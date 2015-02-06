using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class MessageHeaders : IMessageHeaders
    {
        public Guid MessageId { get; set; }
        public Dictionary<string, string> CustomHeaders { get; set; }

        protected MessageHeaders()
        {
            MessageId = Guid.NewGuid();
            CustomHeaders = new Dictionary<string, string>();
        }
    }
}