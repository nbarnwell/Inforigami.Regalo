using System;

namespace Inforigami.Regalo.Core
{
    public abstract class EventHandlingResultEvent
    {
        public Guid CorrelationId { get; set; }
    }
}
