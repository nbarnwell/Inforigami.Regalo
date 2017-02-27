using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public class EventStream<TAggregateRoot>
    {
        private List<IEvent> _events;

        public string Id { get; private set; }

        public IEnumerable<IEvent> Events
        {
            get { return _events.ToArray(); }
            private set { _events = new List<IEvent>(value); }
        }

        public bool HasEvents { get { return _events != null && _events.Any(); } }

        public EventStream(string id)
        {
            Id = id;
            _events = new List<IEvent>();
        }

        public int GetVersion()
        {
            return _events.Last().Version;
        }

        public void Append(IEnumerable<IEvent> events)
        {
            _events.AddRange(events);
        }
    }
}