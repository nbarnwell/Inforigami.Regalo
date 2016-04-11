using System;

namespace Inforigami.Regalo.Interfaces
{
    public interface IEvent : IMessage
    {
        Guid CausationId { get; set; }
        Guid CorrelationId { get; set; }
        int Version { get; set; }
    }
}
