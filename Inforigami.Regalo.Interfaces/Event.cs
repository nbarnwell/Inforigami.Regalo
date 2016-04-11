using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Event : IEvent
    {
        public Guid           MessageId     { get; set; }
        public DateTimeOffset Timestamp     { get; set; }
        public Guid           CausationId   { get; set; }
        public Guid           CorrelationId { get; set; }
        public int            Version       { get; set; }

        public Event()
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTimeOffset.Now;
        }
    }
}