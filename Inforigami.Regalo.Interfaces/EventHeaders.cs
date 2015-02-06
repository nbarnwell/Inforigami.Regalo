using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public class EventHeaders : MessageHeaders, IEventHeaders
    {
        public Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }
        public Dictionary<string, string> Topics { get; set; }

        public EventHeaders()
        {
            CausationId = MessageId;
            CorrelationId = MessageId;
            Version = 1;
            Topics = new Dictionary<string, string>();
        }
    }
}