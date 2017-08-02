using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Message : IMessage
    {
        public Guid MessageId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTimeOffset CorrelationTimestamp { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        protected Message()
        {
            MessageId            = Guid.NewGuid();
            Timestamp            = DateTimeOffset.Now;
            CausationId          = MessageId;
            CorrelationId        = MessageId;
            CorrelationTimestamp = Timestamp;
            Tags                 = new Dictionary<string, string>();
        }
    }
}