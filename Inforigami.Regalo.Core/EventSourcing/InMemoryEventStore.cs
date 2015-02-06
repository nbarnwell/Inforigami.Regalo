using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly IDictionary<Guid, IList<IEvent>> _aggregates = new Dictionary<Guid, IList<IEvent>>();

        public IEnumerable<IEvent> Load(Guid aggregateId)
        {
            return GetAggregateEventList(aggregateId);
        }

        public IEnumerable<IEvent> Load(Guid aggregateId, int maxVersion)
        {
            IList<IEvent> events = GetAggregateEventList(aggregateId);
            return events.Where(x => x.Headers.Version <= maxVersion);
        }

        public void Add(Guid aggregateId, IEnumerable<IEvent> events)
        {
            Update(aggregateId, events);
        }

        public void Update(Guid aggregateId, IEvent evt)
        {
            IList<IEvent> aggregateEventList = GetAggregateEventList(aggregateId);
            var lastEvent = aggregateEventList.LastOrDefault();

            if (lastEvent != null)
            {
                evt.Headers.Version = lastEvent.Headers.Version + 1;
            }

            aggregateEventList.Add(evt);
        }

        public void Update(Guid aggregateId, IEnumerable<IEvent> events)
        {
            IList<IEvent> aggregateEventList = GetAggregateEventList(aggregateId);
            var lastEvent = aggregateEventList.LastOrDefault();

            foreach (var evt in events)
            {
                if (lastEvent != null)
                {
                    evt.Headers.Version = lastEvent.Headers.Version + 1;
                }

                lastEvent = evt;

                aggregateEventList.Add(evt);
            }
        }

        private IList<IEvent> FindAggregateEventList(Guid aggregateId)
        {
            IList<IEvent> aggregateEventList;
            if (!_aggregates.TryGetValue(aggregateId, out aggregateEventList))
            {
                return null;
            }
            return aggregateEventList;
        }

        private IList<IEvent> GetAggregateEventList(Guid aggregateId)
        {
            IList<IEvent> aggregateEventList = FindAggregateEventList(aggregateId);
            if (aggregateEventList == null)
            {
                aggregateEventList = new List<IEvent>();
                _aggregates.Add(aggregateId, aggregateEventList);
            }
            return aggregateEventList;
        }

        public IEnumerable<IEvent> Events
        {
            get { return _aggregates.Values.SelectMany(list => list).ToArray(); }
        }
    }
}
