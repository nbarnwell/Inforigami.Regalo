using System;
using System.Collections.Generic;
using System.Linq;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly IDictionary<Guid, IList<Event>> _aggregates = new Dictionary<Guid, IList<Event>>();

        public IEnumerable<Event> Load(Guid aggregateId)
        {
            return GetAggregateEventList(aggregateId);
        }

        public IEnumerable<Event> Load(Guid aggregateId, int maxVersion)
        {
            IList<Event> events = GetAggregateEventList(aggregateId);
            return events.Where(x => x.Version <= maxVersion);
        }

        public void Add(Guid aggregateId, IEnumerable<Event> events)
        {
            Update(aggregateId, events);
        }

        public void Update(Guid aggregateId, Event evt)
        {
            IList<Event> aggregateEventList = GetAggregateEventList(aggregateId);
            var lastEvent = aggregateEventList.LastOrDefault();

            if (lastEvent != null)
            {
                evt.Follows(lastEvent);
            }

            aggregateEventList.Add(evt);
        }

        public void Update(Guid aggregateId, IEnumerable<Event> events)
        {
            IList<Event> aggregateEventList = GetAggregateEventList(aggregateId);
            var lastEvent = aggregateEventList.LastOrDefault();

            foreach (var evt in events)
            {
                if (lastEvent != null)
                {
                    evt.Follows(lastEvent);
                }

                lastEvent = evt;

                aggregateEventList.Add(evt);
            }
        }

        private IList<Event> FindAggregateEventList(Guid aggregateId)
        {
            IList<Event> aggregateEventList;
            if (!_aggregates.TryGetValue(aggregateId, out aggregateEventList))
            {
                return null;
            }
            return aggregateEventList;
        }

        private IList<Event> GetAggregateEventList(Guid aggregateId)
        {
            IList<Event> aggregateEventList = FindAggregateEventList(aggregateId);
            if (aggregateEventList == null)
            {
                aggregateEventList = new List<Event>();
                _aggregates.Add(aggregateId, aggregateEventList);
            }
            return aggregateEventList;
        }

        public IEnumerable<Event> Events
        {
            get { return _aggregates.Values.SelectMany(list => list).ToArray(); }
        }
    }
}
