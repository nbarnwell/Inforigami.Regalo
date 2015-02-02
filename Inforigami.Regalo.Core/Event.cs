using System;
using System.Diagnostics;

namespace Inforigami.Regalo.Core
{
    public abstract class Event : Message
    {
        public int Version { get; set; }

        protected Event()
        {
            Version = 1;
        }

        public Event CausedBy(Command message)
        {
            CausationId = message.Id;
            CorrelationId = message.CorrelationId;

            return this;
        }

        public Event CausedBy(Event message)
        {
            CausationId = message.Id;
            CorrelationId = message.CorrelationId;

            return this;
        }

        public Event Follows(Event evt)
        {
            Version = evt.Version + 1;
            return this;
        }
    }
}
