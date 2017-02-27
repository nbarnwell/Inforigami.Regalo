using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Messaging
{
    public static class EventHandlingSucceededEvent
    {
        public static IEventHandlingSucceededEvent<TEvent> Create<TEvent>(TEvent evt)
            where TEvent : IEvent
        {
            return (IEventHandlingSucceededEvent<TEvent>)WrapEvent(evt);
        }

        private static object WrapEvent(IEvent evt)
        {
            var wrapperType = typeof(EventHandlingSucceededEvent<>).MakeGenericType(evt.GetType());
            return Activator.CreateInstance(wrapperType, evt);
        }

    }
}
