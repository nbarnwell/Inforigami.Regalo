using System.Collections;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class EventChain : IEnumerable<Event>
    {
        private readonly IList<Event> _events = new List<Event>();

        private Event _lastEvent;

        public EventChain Add(Event evt)
        {
            if (_lastEvent != null)
            {
                evt.Follows(_lastEvent);
            }

            _lastEvent = evt;

            _events.Add(evt);

            return this;
        }

        public IEnumerator<Event> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Event this[int index]
        {
            get { return _events[index]; }
        }
    }
}