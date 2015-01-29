using System;

namespace Inforigami.Regalo.Core
{
    public interface IEventHandlingFailedEvent<out TEvent>
    {
        TEvent Evt { get; }
        Exception Exception { get; }
    }
}
