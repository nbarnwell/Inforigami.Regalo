using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Raven.Client;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Session;
using Conventions = Inforigami.Regalo.Core.Conventions;

namespace Inforigami.Regalo.RavenDB
{
    public class RavenEventStore : IEventStore, IEventStoreWithPreloading, IDisposable
    {
        private bool _hasChanges;
        private IDocumentSession _documentSession;

        public RavenEventStore(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException("documentStore");

            _documentSession = documentStore.OpenSession();
            _documentSession.Advanced.UseOptimisticConcurrency = true;
        }

        public void Save<T>(string eventStreamId, int expectedVersion, IEnumerable<IEvent> events)
        {
            var stream = _documentSession.Load<EventStream>(eventStreamId);

            if (stream == null)
            {
                stream = new EventStream(eventStreamId);
                stream.Append(events);
                _documentSession.Store(stream);

                SetRavenCollectionName(events, _documentSession, stream);
            }
            else
            {
                stream.Append(events);
            }

            _hasChanges = true;
        }

        public void Flush()
        {
            _documentSession.SaveChanges();
            _hasChanges = false;
        }

        private static void SetRavenCollectionName(IEnumerable<object> events, IDocumentSession session, EventStream stream)
        {
            if (Conventions.FindAggregateTypeForEventType != null)
            {
                var aggregateType = Conventions.FindAggregateTypeForEventType(events.First().GetType());
                session.Advanced.GetMetadataFor(stream)["Raven-Entity-Name"] =
                    DocumentConventions.DefaultGetCollectionName(aggregateType);
            }
        }

        public void Preload(IEnumerable<string> aggregateIds)
        {
            _documentSession.Load<EventStream>(aggregateIds);
        }

        public EventStream<T> Load<T>(string eventStreamId)
        {
            var stream = _documentSession.Load<EventStream>(eventStreamId);

            var result = new EventStream<T>(eventStreamId);

            if (stream != null)
            {
                result.Append(stream.Events);
            }

            return result;
        }

        public EventStream<T> Load<T>(string eventStreamId, int maxVersion)
        {
            var events = Load<T>(eventStreamId).Events.ToList();

            if (maxVersion != EntityVersion.Latest && events.All(x => x.Version != maxVersion))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxVersion),
                    maxVersion,
                    $"BaseVersion {maxVersion} not found for aggregate {eventStreamId}");
            }

            var result = new EventStream<T>(eventStreamId);
            result.Append(GetEventsForVersion(events, maxVersion));
            return result;
        }

        [Obsolete("Use Delete<T> instead", true)]
        public void Delete(string eventStreamId, int version)
        {
            throw new NotImplementedException("Replaced by Delete<T>");
        }

        public void Delete<T>(string eventStreamId, int expectedVersion)
        {
            var stream = _documentSession.Load<EventStream>(eventStreamId);
            var actualVersion = stream.Events.Last().Version;
            if (actualVersion != expectedVersion)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version {1}", expectedVersion, actualVersion));
                exception.Data.Add("Existing stream", eventStreamId);
                throw exception;
            }

            _documentSession.Delete(stream);
        }

        public void Commit()
        {
            Flush();
        }

        public void Rollback()
        {
            if (_hasChanges)
            {
                throw new InvalidOperationException(
                    "Disposing a delayed-write event store with pending changes. Be sure to call Flush() when all operations are completed.");
            }
        }

        private static IEnumerable<IEvent> GetEventsForVersion(IEnumerable<IEvent> events, int maxVersion)
        {
            return events.Where(x => maxVersion == EntityVersion.Latest || x.Version <= maxVersion);
        }

        public void Dispose()
        {
            if (_documentSession != null)
            {
                Rollback();

                _documentSession.Dispose();
                _documentSession = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
