using System;

namespace Inforigami.Regalo.Interfaces
{
    public interface IMessage
    {
        Guid Id { get; set; }
        Guid CausationId { get; set; }
        Guid CorrelationId { get; set; }
    }
}
