using System;

namespace Inforigami.Regalo.Core
{
    public static class EventHandlingFailedEvent
    {
        public static IEventHandlingFailedEvent<TEvent> Create<TEvent>(TEvent evt, Exception exception)
            where TEvent : Event
        {
            return (IEventHandlingFailedEvent<TEvent>)WrapEvent(evt, exception);
        }

        private static object WrapEvent(Event evt, Exception exception)
        {
            var wrapperType = typeof(EventHandlingFailedEventImpl<>).MakeGenericType(evt.GetType());
            return Activator.CreateInstance(wrapperType, evt, exception);
        }

        private class EventHandlingFailedEventImpl<TEvent> : EventHandlingResultEvent, IEventHandlingFailedEvent<TEvent>
        {
            public TEvent Evt { get; private set; }
            public Exception Exception { get; private set; }

            public EventHandlingFailedEventImpl(TEvent evt, Exception exception)
            {
                Evt = evt;
                Exception = exception;
            }
        }
    }
}
