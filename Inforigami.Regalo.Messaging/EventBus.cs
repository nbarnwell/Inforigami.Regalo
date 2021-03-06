using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Messaging
{
    public class EventBus : MessageProcessorBase, IEventBus
    {
        private readonly ILogger _logger;

        public EventBus(ILogger logger, INoHandlerFoundStrategyFactory noHandlerFoundStrategyFactory) 
            : base(logger, noHandlerFoundStrategyFactory)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public void Publish<TEvent>(TEvent evt)
            where TEvent : IEvent
        {
            var eventType = evt.GetType();

            try
            {
                HandleMessage(evt, typeof(IEventHandler<>));
            }
            catch (Exception e)
            {
                _logger.Error(this, e, "Failed to handle {0}", evt);

                if (ExceptionShouldBubble(evt, e))
                {
                    throw;
                }

                if (eventType != typeof(object))
                {
                    var failedEvent = EventHandlingFailedEvent.Create(evt, e);
                    _logger.Error(this, e, "Failed to handle {0}, publishing {1}...", evt, failedEvent);
                    HandleMessage(failedEvent, typeof(IEventHandler<>));
                }
                else
                {
                    _logger.Error(this, e, "Failed to handle {0} but NOT publishing EventHandlingFailedEvent<object>...", evt);
                }
                return;
            }

            if (eventType != typeof(object))
            {
                var succeededEvent = EventHandlingSucceededEvent.Create(evt);
                _logger.Debug(this, "Handled {0}, publishing {1}...", evt, succeededEvent);
                HandleMessage(succeededEvent, typeof(IEventHandler<>));
            }
            else
            {
                _logger.Debug(this, "Handled {0} but NOT publishing EventHandlingSucceededEvent<object>...", evt);
            }
        }

        public void Publish<TEvent>(IEnumerable<TEvent> events)
            where TEvent : IEvent
        {
            foreach (var evt in events)
            {
                Publish(evt);
            }
        }

        private static bool ExceptionShouldBubble(object evt, Exception exception)
        {
            var filter = Conventions.EventPublishingExceptionFilter;
            return filter != null && filter(evt, exception);
        }
    }
}
