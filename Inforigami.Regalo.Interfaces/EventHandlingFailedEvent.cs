using System;

namespace Inforigami.Regalo.Interfaces
{
    public class EventHandlingFailedEvent<TEvent> : Event, IEventHandlingFailedEvent<TEvent>
    {
        public TEvent Evt { get; private set; }
        public Exception Exception { get; private set; }

        public EventHandlingFailedEvent(TEvent evt, Exception exception)
        {
            Evt = evt;
            Exception = exception;
        }
    }
}