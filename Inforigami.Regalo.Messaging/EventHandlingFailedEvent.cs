using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Messaging
{
    public static class EventHandlingFailedEvent
    {
        public static IEventHandlingFailedEvent<TEvent> Create<TEvent>(TEvent evt, Exception exception)
            where TEvent : IEvent
        {
            return (IEventHandlingFailedEvent<TEvent>)WrapEvent(evt, exception);
        }

        private static object WrapEvent(IEvent evt, Exception exception)
        {
            var wrapperType = typeof(EventHandlingFailedEvent<>).MakeGenericType(evt.GetType());
            return Activator.CreateInstance(wrapperType, evt, exception);
        }

    }
}
