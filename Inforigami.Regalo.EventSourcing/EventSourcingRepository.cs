using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public class EventSourcingRepository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot, new()
    {
        private readonly ILogger _logger;
        private readonly IEventStore _eventStore;
        private readonly IConcurrencyMonitor _concurrencyMonitor;

        public EventSourcingRepository(IEventStore eventStore, IConcurrencyMonitor concurrencyMonitor, ILogger logger)
        {
            _eventStore         = eventStore         ?? throw new ArgumentNullException(nameof(eventStore));
            _concurrencyMonitor = concurrencyMonitor ?? throw new ArgumentNullException(nameof(concurrencyMonitor));
            _logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
        }

        public TAggregateRoot Get(Guid id, int version)
        {
            return Get(id, (int?)version);
        }

        private TAggregateRoot Get(Guid id, int? version)
        {
            var aggregateId = EventStreamIdFormatter.GetStreamId<TAggregateRoot>(id.ToString());
            var entityVersion = version ?? EntityVersion.Latest;
            _logger.Debug(this, $"Loading stream {aggregateId}@{EntityVersion.GetName(entityVersion)}");

            var stream = _eventStore.Load<TAggregateRoot>(aggregateId, entityVersion);

            if (stream == null)
            {
                _logger.Warn(this, "No stream found for ID {0}", aggregateId);
                return null;
            }

            if (!stream.HasEvents)
            {
                _logger.Warn(this, "Stream for ID {0} has no events for version {1}", aggregateId, EntityVersion.GetName(entityVersion));
            }

            var aggregateRoot = new TAggregateRoot();

            aggregateRoot.ApplyAll(stream.Events);

            return aggregateRoot;
        }

        public void Save(TAggregateRoot item)
        {
            var uncommittedEvents = item.GetUncommittedEvents().ToArray();

            if (uncommittedEvents.Length == 0) return;

            var streamId = EventStreamIdFormatter.GetStreamId<TAggregateRoot>(item.Id.ToString());

            try
            {
                _eventStore.Save<TAggregateRoot>(streamId, item.BaseVersion, uncommittedEvents);
            }
            catch (EventStoreConcurrencyException)
            {
                EventStream<TAggregateRoot> stream = _eventStore.Load<TAggregateRoot>(streamId);

                if (stream != null)
                {
                    IEvent[] baseAndUnseenEvents = stream.Events.ToArray();

                    if (baseAndUnseenEvents.Length > 0)
                    {
                        var unseenEvents = GetUnseenEvents(item, baseAndUnseenEvents);
                        var conflicts = _concurrencyMonitor.CheckForConflicts(unseenEvents, uncommittedEvents);
                        if (!conflicts.Any())
                        {
                            // Re-version our uncommitted events on top of those already saved to the db.
                            int unseenVersion = unseenEvents.Last().Version;
                            int newVersion = unseenVersion;

                            foreach (var evt in uncommittedEvents)
                            {
                                evt.Version = ++newVersion;
                            }

                            _eventStore.Save<TAggregateRoot>(streamId, unseenVersion, uncommittedEvents);
                        }
                        else
                        {
                            var exception = new ConcurrencyConflictsDetectedException(conflicts);
                            exception.Data.Add("AggregateId", item.Id);
                            throw exception;
                        }
                    }
                }
            }

            item.AcceptUncommittedEvents();
        }

        private static IEnumerable<IEvent> GetUnseenEvents(TAggregateRoot item, IEnumerable<IEvent> baseAndUnseenEvents)
        {
            return baseAndUnseenEvents.Where(x => x.Version > item.BaseVersion);
        }
    }
}
