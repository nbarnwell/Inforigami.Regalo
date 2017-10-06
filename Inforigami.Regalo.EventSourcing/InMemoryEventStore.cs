using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ILogger _logger;
        private readonly IDictionary<string, IList<IEvent>> _eventStreams = new Dictionary<string, IList<IEvent>>();

        public InMemoryEventStore(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        public IEnumerable<IEvent> GetAllEvents()
        {
            return _eventStreams.Values.SelectMany(x => x);
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (string.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException("aggregateId");
            if (expectedVersion < -1)                   throw new ArgumentOutOfRangeException("expectedVersion", "Expected version should be greater than or equal to -1.");
            if (newEvents == null)                      throw new ArgumentNullException("newEvents");

            LogSavingNewEventsList(aggregateId, expectedVersion, newEvents);

            var newEventsList = newEvents.ToList();
            var existingStream = FindEvents(aggregateId);

            CheckConcurrency(expectedVersion, existingStream);

            if (existingStream == null)
            {
                _eventStreams.Add(aggregateId, newEventsList);
            }
            else
            {
                foreach (var evt in newEventsList)
                {
                    existingStream.Add(evt);
                }
            }
        }

        private void LogSavingNewEventsList(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            var eventsList = string.Join(Environment.NewLine, newEvents.Select(x => $"{x.GetType()}: {x.MessageId}"));
            var message = $"Saving events for {aggregateId}@{expectedVersion}{Environment.NewLine}{eventsList}";
            _logger.Debug(this, message);
        }

        private static void CheckConcurrency(int expectedVersion, ICollection<IEvent> existingStream)
        {
            if (existingStream != null && expectedVersion == -1)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected to create a new stream but one already exists."));
                exception.Data.Add("Existing stream", existingStream);
                throw exception;
            }

            if (existingStream != null && existingStream.Last().Version != expectedVersion)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version {1}", expectedVersion, existingStream.Count));
                exception.Data.Add("Existing stream", existingStream);
                throw exception;
            }
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            var events = FindEvents(aggregateId);

            return BuildEventStream<T>(aggregateId, events);
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            var events = FindEvents(aggregateId, version);

            return BuildEventStream<T>(aggregateId, events);
        }

        public void Delete(string aggregateId, int version)
        {
            var events = FindEvents(aggregateId, version);

            if (events.Last().Version != version)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version {1}", version, events.Count));
                exception.Data.Add("Existing stream", aggregateId);
                throw exception;
            }
        }

        public void Delete<T>(string aggregateId)
        {
            _eventStreams.Remove(aggregateId);
        }

        private static EventStream<T> BuildEventStream<T>(string aggregateId, IList<IEvent> events)
        {
            if (events == null)
            {
                return null;
            }

            var stream = new EventStream<T>(aggregateId);
            stream.Append(events);
            return stream;
        }

        private IList<IEvent> FindEvents(string id, int? version = null)
        {
            IList<IEvent> events;
            if (!_eventStreams.TryGetValue(id, out events))
            {
                return null;
            }

            if (version == EventStreamVersion.NoStream)
            {
                throw new ArgumentOutOfRangeException("version", "By definition you cannot load a stream when specifying the EventStreamVersion.NoStream (-1) value.");
            }

            if (version.HasValue && version != EventStreamVersion.Max)
            {
                events = events.Where(x => x.Version <= version.Value).ToList();
            }

            return events;
        }
    }
}
