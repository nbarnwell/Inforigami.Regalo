using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public class EventStream<TAggregateRoot>
    {
        private List<IEvent> _events;

        public EventStream(string id)
        {
            Id = id;
            _events = new List<IEvent>();
        }

        public string Id { get; private set; }

        public IEnumerable<IEvent> Events
        {
            get { return _events.ToArray(); }
            private set { _events = new List<IEvent>(value); }
        }

        public void Append(IEnumerable<IEvent> events)
        {
            _events.AddRange(events);
        }
    }
}