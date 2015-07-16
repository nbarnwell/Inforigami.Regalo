using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Event : IEvent
    {
        public IEventHeaders Headers { get; private set; }

        protected Event()
        {
            Headers = new EventHeaders();
        }

        public void OverwriteHeaders(IEventHeaders headers)
        {
            if (headers == null) throw new ArgumentNullException("headers");

            Headers = headers;
        }
    }
}