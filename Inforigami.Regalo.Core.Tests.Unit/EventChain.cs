using System.Collections;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class EventChain : IEnumerable<IEvent>
    {
        private readonly IList<IEvent> _events = new List<IEvent>();

        private IEvent _lastEvent;

        public EventChain Add(IEvent evt)
        {
            if (_lastEvent != null)
            {
                evt.Version = _lastEvent.Version + 1;
            }

            _lastEvent = evt;

            _events.Add(evt);

            return this;
        }

        public IEnumerator<IEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEvent this[int index]
        {
            get { return _events[index]; }
        }
    }
}