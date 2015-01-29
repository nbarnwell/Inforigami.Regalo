using System.Collections;
using System.Collections.Generic;
using Inforigami.Regalo.Core;

namespace Inforigami.Regalo.Testing
{
    public class FakeEventBus : IEventBus
    {
        private readonly IList<object> _events = new List<object>();

        public IEnumerable<object> Events { get { return _events; } }

        public void Publish<TEvent>(TEvent evt)
        {
            _events.Add(evt);
        }

        public void Publish<TEvent>(IEnumerable<TEvent> events)
        {
            foreach (var evt in events)
            {
                Publish(evt);
            }
        }
    }
}
