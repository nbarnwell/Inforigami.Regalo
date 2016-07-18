using System;
using System.Collections;

namespace Inforigami.Regalo.Interfaces
{
    public interface IMessage
    {
        Guid           MessageId     { get; set; }
        Guid           CausationId   { get; set; }
        Guid           CorrelationId { get; set; }
        DateTimeOffset Timestamp     { get; set; }
    }
}

