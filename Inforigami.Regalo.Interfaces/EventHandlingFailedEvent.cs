using System;

namespace Inforigami.Regalo.Interfaces
{
    public class EventHandlingFailedEvent<TEvent> : IEventHandlingFailedEvent<TEvent>
    {
        public IEventHeaders Headers { get; private set; }
        public TEvent Evt { get; private set; }
        public Exception Exception { get; private set; }

        public EventHandlingFailedEvent(TEvent evt, Exception exception)
        {
            Headers = new EventHeaders();
            Evt = evt;
            Exception = exception;
        }

        public void OverwriteHeaders(IEventHeaders headers)
        {
            if (headers == null) throw new ArgumentNullException("headers");

            Headers = headers;
        }
    }
}