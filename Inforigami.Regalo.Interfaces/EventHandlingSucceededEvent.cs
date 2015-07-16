using System;

namespace Inforigami.Regalo.Interfaces
{
    public class EventHandlingSucceededEvent<TEvent> : IEventHandlingSucceededEvent<TEvent>
    {
        public IEventHeaders Headers { get; private set; }
        public TEvent Evt { get; private set; }

        public EventHandlingSucceededEvent(TEvent evt)
        {
            Evt = evt;
            Headers = new EventHeaders();
        }

        public void OverwriteHeaders(IEventHeaders headers)
        {
            if (headers == null) throw new ArgumentNullException("headers");

            Headers = headers;
        }
    }
}