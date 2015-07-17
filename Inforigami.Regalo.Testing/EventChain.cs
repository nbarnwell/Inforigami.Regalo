using System.Collections;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class EventChain : IEnumerable<IEvent>
    {
        private readonly IList<IEvent> _events = new List<IEvent>();

        private int _version = -1;

        public EventChain()
        {
        }

        public EventChain(int startVersion)
        {
            _version = startVersion;
        }

        public EventChain Add(IEvent evt)
        {
            evt.Headers.Version = ++_version;

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