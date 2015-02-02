using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Newtonsoft.Json;

namespace Inforigami.Regalo.RavenDB
{
    public class EventStream
    {
        private List<Event> _events;

        public EventStream(string id)
        {
            Id = id;
            _events = new List<Event>();
        }

        public string Id { get; private set; }

        public IEnumerable<Event> Events
        {
            get { return _events.ToArray(); }
            private set { _events = new List<Event>(value); }
        }

        public void Append(IEnumerable<Event> events)
        {
            _events.AddRange(events);
        }
    }
}
