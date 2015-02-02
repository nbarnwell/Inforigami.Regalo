using System;
using System.Diagnostics;

namespace Inforigami.Regalo.Core
{ 
    public abstract class Event : Message, IEvent
    {
        public int Version { get; set; }

        protected Event()
        {
            Version = 1;
        }

        public IEvent CausedBy(ICommand message)
        {
            CausationId = message.Id;
            CorrelationId = message.CorrelationId;

            return this;
        }

        public IEvent CausedBy(IEvent message)
        {
            CausationId = message.Id;
            CorrelationId = message.CorrelationId;

            return this;
        }

        public IEvent Follows(IEvent evt)
        {
            Version = evt.Version + 1;
            return this;
        }
    }

    public interface IEvent : IMessage
    {
        int Version { get; set; }
    }
}
