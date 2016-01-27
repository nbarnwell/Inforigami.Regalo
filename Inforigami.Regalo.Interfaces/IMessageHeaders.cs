using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public interface IMessageHeaders
    {
        Guid MessageId { get; set; }
        DateTimeOffset Timestamp { get; set; }
        Dictionary<string, string> CustomHeaders { get; set; }
    }
}