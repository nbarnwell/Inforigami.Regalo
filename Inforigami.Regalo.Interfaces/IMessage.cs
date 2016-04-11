using System;
using System.Collections;

namespace Inforigami.Regalo.Interfaces
{
    public interface IMessage
    {
        Guid MessageId { get; set; }
        DateTimeOffset Timestamp { get; set; }
    }
}

