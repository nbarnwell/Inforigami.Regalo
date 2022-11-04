using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
using Newtonsoft.Json;
using ILogger = Inforigami.Regalo.Core.ILogger;

namespace Inforigami.Regalo.EventStore
{
    public class EventStoreEventStore : IEventStore, IDisposable
    {
        private bool _committed;

        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IDictionary<string, EventStoreTransaction> _transactions = new Dictionary<string, EventStoreTransaction>();
        private readonly IDictionary<string, IList<IEvent>> _streamCache = new Dictionary<string, IList<IEvent>>();
        private readonly ILogger _logger;

        public EventStoreEventStore(IEventStoreConnection eventStoreConnection, ILogger logger)
        {
            if (eventStoreConnection == null) throw new ArgumentNullException("eventStoreConnection");
            if (logger == null) throw new ArgumentNullException("logger");

            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            var eventStoreExpectedVersion = expectedVersion == -1 ? ExpectedVersion.NoStream : expectedVersion;

            try
            {
                _logger.Debug(this, "Saving " + typeof(T) + " to stream " + aggregateId);

                var transaction = GetTransaction(aggregateId, eventStoreExpectedVersion);
                transaction.WriteAsync(GetEventData(newEvents)).Wait();

                CacheNewEvents(aggregateId, expectedVersion, newEvents);
            }
            catch (WrongExpectedVersionException ex)
            {
                // Wrap in a Regalo-defined exception so that callers don't have to worry what impl is in place
                var concurrencyException = new EventStoreConcurrencyException("Unable to save to EventStore", ex);
                throw concurrencyException;
            }
        }

        private void CacheNewEvents(string aggregateId, int expectedVersion, IEnumerable<IEvent> eventsToAppend)
        {
            IList<IEvent> cachedEvents;
            if (!_streamCache.TryGetValue(aggregateId, out cachedEvents))
            {
                cachedEvents = new List<IEvent>();
                _streamCache[aggregateId] = cachedEvents;
            }

            foreach (var evt in eventsToAppend)
            {
                if (cachedEvents.IsEmpty() || evt.Version == cachedEvents.Last().Version + 1)
                {
                    cachedEvents.Add(evt);
                }
                else
                {
                    var existingVersions = string.Join(", ", cachedEvents.Select(x => x.Version.ToString()));
                    var newVersions = string.Join(", ", eventsToAppend.Select(x => x.Version.ToString()));
                    throw new EventStoreConcurrencyException(
                        $"Failed to update session cache for stream {aggregateId} given expected "
                        + $"version {expectedVersion}, appending versions {newVersions} to {existingVersions}");
                }
            }
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            return Load<T>(aggregateId, EntityVersion.Latest);
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            if (string.IsNullOrWhiteSpace(aggregateId))
                throw new ArgumentException("An aggregate ID is required", "aggregateId");

            if (version == EntityVersion.New)
            {
                throw new ArgumentOutOfRangeException(
                    "version",
                    "By definition you cannot load a stream when specifying the EntityVersion.New (-1) value.");
            }

            var domainEvents =
                LoadEventsFromEventStore<T>(aggregateId, version)
                    .Concat(LoadEventsFromCache<T>(aggregateId, version));

            if (!domainEvents.Any())
            {
                return null;
            }

            var result = new EventStream<T>(aggregateId);
            result.Append(domainEvents);

            if (version != EntityVersion.Latest && result.GetVersion() != version)
            {
                var exception = new ArgumentOutOfRangeException(
                    "version",
                    version,
                    string.Format("Event for version {0} could not be found for stream {1}", EntityVersion.GetName(version), aggregateId));
                throw exception;
            }

            return result;
        }

        private IEnumerable<IEvent> LoadEventsFromEventStore<T>(string aggregateId, int version)
        {
            _logger.Debug(this, "Loading " + typeof(T) + " version " + EntityVersion.GetName(version) + " from stream " + aggregateId);

            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var               nextSliceStart = 0L;
            do
            {
                currentSlice = _eventStoreConnection
                               .ReadStreamEventsForwardAsync(aggregateId, nextSliceStart, 200, false)
                               .Result;

                nextSliceStart = currentSlice.NextEventNumber;

                foreach (var evt in currentSlice.Events)
                {
                    streamEvents.Add(evt);
                    if (evt.OriginalEventNumber == version)
                    {
                        break;
                    }
                }
            } while (!currentSlice.IsEndOfStream);

            var events = streamEvents.Select(x => BuildDomainEvent(x.OriginalEvent.Data, x.OriginalEvent.Metadata))
                                     .ToList();

            return events;
        }

        private IEnumerable<IEvent> LoadEventsFromCache<T>(string aggregateId, int version)
        {
            IList<IEvent> cachedEvents;
            if (_streamCache.TryGetValue(aggregateId, out cachedEvents))
            {
                _logger.Debug(this, "Loading " + typeof(T) + " version " + EntityVersion.GetName(version) + " from cache " + aggregateId);
                return cachedEvents.Where(x => version == EntityVersion.Latest || x.Version <= version)
                                   .ToList();
            }

            return new IEvent[0];
        }

        [Obsolete("Use Delete<T> instead", true)]
        public void Delete(string aggregateId, int version)
        {
            throw new NotImplementedException("Replaced by Delete<T>");
        }

        public void Delete<T>(string aggregateId, int expectedVersion)
        {
            _eventStoreConnection.DeleteStreamAsync(aggregateId, expectedVersion).Wait();
        }

        public void Rollback()
        {
            _logger.Warn(this, "Rolling-back changes to EventStore...");

            foreach (var transaction in _transactions.Values)
            {
                transaction.Rollback();
            }

            _streamCache.Clear();
            _transactions.Clear();
        }

        public void Flush()
        {
            _committed = true;

            var transactionsToCommit = _transactions.Values.ToArray();

            _logger.Debug(
                this,
                "Committing EventStore transactions for {0} streams: [{1}]...",
                transactionsToCommit.Length,
                string.Join(", ", _transactions.Keys));

            Task.WaitAll(
                transactionsToCommit.Select(x => x.CommitAsync())
                                    .Cast<Task>()
                                    .ToArray());

            _streamCache.Clear();
            _transactions.Clear();
        }

        private static IEvent BuildDomainEvent(byte[] data, byte[] metadata)
        {
            var dataJson = Encoding.UTF8.GetString(data);

            var evt = (IEvent)JsonConvert.DeserializeObject(dataJson, GetDefaultJsonSerializerSettings());

            return evt;
        }

        public void Dispose()
        {
            if (!_committed)
            {
                Rollback();
            }
        }

        private static IEnumerable<EventData> GetEventData(IEnumerable<IEvent> newEvents)
        {
            return newEvents.Select(
                x => new EventData(
                    x.MessageId,
                    GetEventTypeFriendlyName(x),
                    true,
                    GetEventBytes(x),
                    null)); }

        private static byte[] GetEventBytes(IEvent evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            return GetBytes(evt);
        }

        private static byte[] GetBytes(object item)
        {
            var json = JsonConvert.SerializeObject(item, Formatting.None, GetDefaultJsonSerializerSettings());
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        private static JsonSerializerSettings GetDefaultJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();

            settings.TypeNameHandling = TypeNameHandling.All;

            settings.Converters.Add(new DomainValueConverter<Guid>(Guid.Parse));
            settings.Converters.Add(new DomainValueConverter<int>(int.Parse));
            settings.Converters.Add(new DomainValueConverter<string>(s => s));

            return settings;
        }

        private static string GetEventTypeFriendlyName(IEvent evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            return evt.GetType().Name.ToCamelCase();
        }

        private EventStoreTransaction GetTransaction(string streamId, int expectedVersion)
        {
            EventStoreTransaction transaction;
            if (!_transactions.TryGetValue(streamId, out transaction))
            {
                _logger.Debug(this, $"Starting new transaction for stream {streamId} at expected version {expectedVersion}");
                transaction = _eventStoreConnection.StartTransactionAsync(streamId, expectedVersion).Result;
                _transactions.Add(streamId, transaction);
            }

            return transaction;
        }
    }
}