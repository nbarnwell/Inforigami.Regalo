using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public interface IEventHeaders : IMessageHeaders
    {
        Guid CausationId { get; set; }
        Guid CorrelationId { get; set; }
        int Version { get; set; }
        Dictionary<string, string> Topics { get; set; }
    }
}