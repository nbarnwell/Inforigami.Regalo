using System;
using System.Collections;
using System.Text;

namespace Inforigami.Regalo.Core
{
    public abstract class Message
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; }

        protected Message()
        {
            Id            = GuidProvider.NewGuid();
            CausationId   = Id;
            CorrelationId = Id;
        }
    }
}
