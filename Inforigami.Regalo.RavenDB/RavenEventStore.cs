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
                session.Advanced.GetMetadataFor(stream)["Raven-Entity-Name"] =
                    DocumentConventions.DefaultGetCollectionName(aggregateType);
            }
        }

        public void Preload(IEnumerable<Guid> aggregateIds)
        {
            var idValues = aggregateIds.Select(x => x.ToString());
            _documentSession.Load<EventStream>(idValues);
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            var stream = _documentSession.Load<EventStream>(aggregateId);

            if (stream == null) return null;

            var result = new EventStream<T>(aggregateId);
            result.Append(stream.Events);
            return result;
        }

        public EventStream<T> Load<T>(string aggregateId, int maxVersion)
        {
            var events = Load<T>(aggregateId).Events.ToList();

            if (maxVersion != EntityVersion.Latest && events.All(x => x.Version != maxVersion))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxVersion),
                    maxVersion,
                    $"BaseVersion {maxVersion} not found for aggregate {aggregateId}");
            }

            var result = new EventStream<T>(aggregateId);
            result.Append(GetEventsForVersion(events, maxVersion));
            return result;
        }

        [Obsolete("Use Delete<T> instead", true)]
        public void Delete(string aggregateId, int version)
        {
            throw new NotImplementedException("Replaced by Delete<T>");
        }

        public void Delete<T>(string aggregateId, int expectedVersion)
        {
            var stream = _documentSession.Load<EventStream>(aggregateId);
            var actualVersion = stream.Events.Last().Version;
            if (actualVersion != expectedVersion)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version {1}", expectedVersion, actualVersion));
                exception.Data.Add("Existing stream", aggregateId);
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
