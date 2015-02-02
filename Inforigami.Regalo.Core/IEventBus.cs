using System.Collections.Generic;

namespace Inforigami.Regalo.Core
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt) where TEvent : IEvent;
        void Publish<TEvent>(IEnumerable<TEvent> events) where TEvent : IEvent;
    }
}
