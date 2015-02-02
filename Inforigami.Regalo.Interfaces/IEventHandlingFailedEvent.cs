using System;

namespace Inforigami.Regalo.Interfaces
{
    public interface IEventHandlingFailedEvent<out TEvent> : IEventHandlingResultEvent
    {
        TEvent Evt { get; }
        Exception Exception { get; }
    }
}
