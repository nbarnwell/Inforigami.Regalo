using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.EventSourcing;

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

        public void Add(Guid aggregateId, IEnumerable<IEvent> events)
        {
            var stream = new EventStream(aggregateId.ToString());
            stream.Append(events);
            _documentSession.Store(stream);

            SetRavenCollectionName(events, _documentSession, stream);

            _hasChanges = true;
        }

        public void Update(Guid aggregateId, IEnumerable<IEvent> events)
        {
            var aggregateIdAsString = aggregateId.ToString();

            var stream = _documentSession.Load<EventStream>(aggregateIdAsString);

            if (stream == null)
            {
                throw new InvalidOperationException("You cannot update an aggregate that has not been saved.");
            }

            stream.Append(events);

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

        public IEnumerable<IEvent> Load(Guid aggregateId)
        {
            var aggregateIdAsString = aggregateId.ToString();

            var stream = _documentSession.Load<EventStream>(aggregateIdAsString);

            if (stream != null)
            {
                var events = stream.Events;

                return events.Any() ? events : Enumerable.Empty<IEvent>();
            }

            return Enumerable.Empty<IEvent>();
        }

        public IEnumerable<IEvent> Load(Guid aggregateId, int maxVersion)
        {
            var events = Load(aggregateId).ToList();

            if (events.All(x => x.Headers.Version != maxVersion))
            {
                throw new ArgumentOutOfRangeException(
                    "maxVersion",
                    maxVersion,
                    string.Format("BaseVersion {0} not found for aggregate {1}", maxVersion, aggregateId));
            }

            return GetEventsForVersion(events, maxVersion).ToList();
        }

        private static IEnumerable<IEvent> GetEventsForVersion(IEnumerable<IEvent> events, int maxVersion)
        {
            return events.Where(x => x.Headers.Version <= maxVersion);
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
