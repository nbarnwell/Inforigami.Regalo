using System;

namespace Inforigami.Regalo.EventSourcing
{
    public class EventStoreConcurrencyException : Exception
    {
        public EventStoreConcurrencyException(string message)
            : base(message)
        {
        }

        public EventStoreConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}