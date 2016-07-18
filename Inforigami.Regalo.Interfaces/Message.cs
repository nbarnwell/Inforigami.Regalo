using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Message : IMessage
    {
        public Guid           MessageId     { get; set; }
        public Guid           CausationId   { get; set; }
        public Guid           CorrelationId { get; set; }
        public DateTimeOffset Timestamp     { get; set; }

        protected Message()
        {
            MessageId     = Guid.NewGuid();
            CausationId   = MessageId;
            CorrelationId = MessageId;
            Timestamp     = DateTimeOffset.Now;
        }
    }
}