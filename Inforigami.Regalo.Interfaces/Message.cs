using System;

namespace Inforigami.Regalo.Interfaces
{
    public class Message : IMessage
    {
        public Guid MessageId { get; set; }
        public Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public Message()
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTimeOffset.Now;
        }
    }
}