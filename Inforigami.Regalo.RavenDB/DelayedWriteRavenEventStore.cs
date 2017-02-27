using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.RavenDB
{
    public class DelayedWriteRavenEventStore : IDelayedWriteEventStore, IEventStoreWithPreloading, IDisposable
    {
        private bool _hasChanges;
        private IDocumentSession _documentSession;

        public DelayedWriteRavenEventStore(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException("documentStore");

            _documentSession = documentStore.OpenSession();
            _documentSession.Advanced.UseOptimisticConcurrency = true;
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> events)
        {
            var aggregateIdAsString = aggregateId.ToString();

            var stream = _documentSession.Load<EventStream>(aggregateIdAsString);

            if (stream == null)
            {
                stream = new EventStream(aggregateId.ToString());
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
                session.Advanced.GetMetadataFor(stream)[Constants.RavenEntityName] =
                    DocumentConvention.DefaultTypeTagName(aggregateType);
            }
        }

        public void Preload(IEnumerable<Guid> aggregateIds)
        {
            var idValues = aggregateIds.Select(x => x.ToString());
            _documentSession.Load<EventStream>(idValues);
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            var aggregateIdAsString = aggregateId.ToString();

            var stream = _documentSession.Load<EventStream>(aggregateIdAsString);

            var result = new EventStream<T>(aggregateIdAsString);

            if (stream != null)
            {
                result.Append(stream.Events);
            }

            return result;
        }

        public EventStream<T> Load<T>(string aggregateId, int maxVersion)
        {
            var events = Load<T>(aggregateId).Events.ToList();

            if (events.All(x => x.Version != maxVersion))
            {
                throw new ArgumentOutOfRangeException(
                    "maxVersion",
                    maxVersion,
                    string.Format("BaseVersion {0} not found for aggregate {1}", maxVersion, aggregateId));
            }

            var result = new EventStream<T>(aggregateId);
            result.Append(GetEventsForVersion(events, maxVersion));
            return result;
        }

        private static IEnumerable<IEvent> GetEventsForVersion(IEnumerable<IEvent> events, int maxVersion)
        {
            return events.Where(x => x.Version <= maxVersion);
        }

        public void Dispose()
        {
            if (_documentSession != null)
            {
                if (_hasChanges)
                {
                    throw new InvalidOperationException("Disposing a delayed-write event store with pending changes. Be sure to call Flush() when all operations are completed.");
                }

                _documentSession.Dispose();
                _documentSession = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
