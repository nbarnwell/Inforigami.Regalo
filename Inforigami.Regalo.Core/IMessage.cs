using System;

namespace Inforigami.Regalo.Core
{
    public abstract class Message : IMessage
    {
        public Guid Id { get; set; }
        public Guid CausationId { get; set; }
        public Guid CorrelationId { get; set; }

        protected Message()
        {
            Id = GuidProvider.NewGuid();
            CausationId = Id;
            CorrelationId = Id;
        }
    }

    public interface IMessage
    {
        Guid Id { get; set; }
        Guid CausationId { get; set; }
        Guid CorrelationId { get; set; }
    }
}
