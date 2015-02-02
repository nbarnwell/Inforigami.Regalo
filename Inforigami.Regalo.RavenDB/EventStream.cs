using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Newtonsoft.Json;

namespace Inforigami.Regalo.RavenDB
{
    public class EventStream
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
