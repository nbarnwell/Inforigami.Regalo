using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Message : IMessage
    {
        public Guid Id            { get; set; }
        public Guid CausationId   { get; set; }
        public Guid CorrelationId { get; set; }

        protected Message()
        {
            Id            = Guid.NewGuid();
            CausationId   = Id;
            CorrelationId = Id;
        }
    }
}