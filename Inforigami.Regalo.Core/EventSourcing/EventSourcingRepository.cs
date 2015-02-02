using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public class EventSourcingRepository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot, new()
    {
        private readonly IEventStore _eventStore;
        private readonly IConcurrencyMonitor _concurrencyMonitor;
        private readonly ISet<Guid> _loaded = new HashSet<Guid>();

        public EventSourcingRepository(IEventStore eventStore, IConcurrencyMonitor concurrencyMonitor)
        {
            _eventStore = eventStore;
            _concurrencyMonitor = concurrencyMonitor;
        }

        public TAggregateRoot Get(Guid id)
        {
            return Get(id, null);
        }

        public TAggregateRoot Get(Guid id, int version)
        {
            return Get(id, (int?)version);
        }

        private TAggregateRoot Get(Guid id, int? version)
        {
            var events =
                version == null
                    ? _eventStore.Load(id)
                    : _eventStore.Load(id, version.Value);

            events = events.ToList();

            if (!events.Any()) return null;

            var aggregateRoot = new TAggregateRoot();

            aggregateRoot.ApplyAll(events);

            _loaded.Add(aggregateRoot.Id);

            return aggregateRoot;
        }

        public void Save(TAggregateRoot item)
        {
            var uncommittedEvents = item.GetUncommittedEvents().ToArray();

            if (uncommittedEvents.Length == 0) return;

            if (_loaded.Contains(item.Id))
            {
                IEvent[] baseAndUnseenEvents = _eventStore.Load(item.Id).ToArray();

                if (baseAndUnseenEvents.Length > 0)
                {
                    var unseenEvents = GetUnseenEvents(item, baseAndUnseenEvents);
                    var conflicts = _concurrencyMonitor.CheckForConflicts(unseenEvents, uncommittedEvents);
                    if (conflicts.Any())
                    {
                        var exception = new ConcurrencyConflictsDetectedException(conflicts);
                        exception.Data.Add("AggregateId", item.Id);
                        throw exception;
                    }
                }

                _eventStore.Update(item.Id, uncommittedEvents);
            }
            else
            {
                _eventStore.Add(item.Id, uncommittedEvents);
                _loaded.Add(item.Id);
            }

            item.AcceptUncommittedEvents();
        }

        private static IEnumerable<IEvent> GetUnseenEvents(TAggregateRoot item, IEnumerable<IEvent> baseAndUnseenEvents)
        {
            return baseAndUnseenEvents.Where(x => x.Version > item.BaseVersion);
        }
    }
}
