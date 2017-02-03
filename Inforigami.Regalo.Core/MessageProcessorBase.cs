using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public abstract class MessageProcessorBase
    {
        private readonly ILogger _logger;
        private readonly INoHandlerFoundStrategyFactory _noHandlerFoundStrategyFactory;

        private readonly object _handleMethodCacheLock = new object();
        private readonly IDictionary<RuntimeTypeHandle, MethodInfo> _handleMethodCache = new Dictionary<RuntimeTypeHandle, MethodInfo>();
        private readonly object _eventHandlingResultEventTypeCacheLock = new object();
        private readonly IDictionary<RuntimeTypeHandle, bool> _eventHandlingResultEventTypeCache = new Dictionary<RuntimeTypeHandle, bool>();

        protected MessageProcessorBase(ILogger logger, INoHandlerFoundStrategyFactory noHandlerFoundStrategyFactory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (noHandlerFoundStrategyFactory == null) throw new ArgumentNullException(nameof(noHandlerFoundStrategyFactory));
            
            _logger = logger;
            _noHandlerFoundStrategyFactory = noHandlerFoundStrategyFactory;
        }

        protected void HandleMessage<TMessage>(TMessage message, Type messageHandlerOpenType)
        {
            var targets = GetHandlerDescriptors(messageHandlerOpenType, message.GetType());

            if (targets.IsEmpty())
            {
                HandleNoHandlerFound(message);
            }
            else
            {
                foreach (var target in targets)
                {
                    _logger.Debug(this, "Invoking {0} with {1}", target.Handler, message);
                    target.MethodInfo.Invoke(target.Handler, new object[] { message });
                }
            }
        }

        private void HandleNoHandlerFound<TMessage>(TMessage message)
        {
            var strategy = _noHandlerFoundStrategyFactory.Create(message);
            strategy.Invoke(message);
        }

        private bool IsEventHandlingResultEvent(Type eventType)
        {
            lock (_eventHandlingResultEventTypeCacheLock)
            {
                bool result;
                if (!_eventHandlingResultEventTypeCache.TryGetValue(eventType.TypeHandle, out result))
                {
                    result = typeof(IEventHandlingResultEvent).IsAssignableFrom(eventType);
                    _eventHandlingResultEventTypeCache.Add(eventType.TypeHandle, result);
                }

                return result;
            }
        }

        private List<HandlerDescriptor> GetHandlerDescriptors(Type messageHandlerOpenType, Type messageType)
        {
            var isEventHandlingResultEvent = IsEventHandlingResultEvent(messageType);

            var messageTypes = isEventHandlingResultEvent
                                   ? GetEventHandlingResultEventTypeHierarchy(messageType)
                                   : GetEventTypeHierarchy(messageType);

            var targets = messageTypes.Select(x => new { MessageType = x, HandlerType = messageHandlerOpenType.MakeGenericType(x) })
                                      .SelectMany(
                                          x => Resolver.ResolveAll(x.HandlerType),
                                          (x, handler) => new HandlerDescriptor
                                          {
                                              MethodInfo = FindHandleMethod(x.MessageType, x.HandlerType),
                                              Handler = handler
                                          })
                                      .ToList();
            return targets;
        }

        private static IEnumerable<Type> GetEventTypeHierarchy(Type eventType)
        {
            var inspector = new TypeInspector();
            return inspector.GetTypeHierarchy(eventType);
        }

        private static IEnumerable<Type> GetEventHandlingResultEventTypeHierarchy(Type type)
        {
            var expectedOpenGenericTypes = new[] { typeof(IEventHandlingSucceededEvent<>), typeof(IEventHandlingFailedEvent<>) };
            var closedGenericType = type.GetInterfaces().Where(i => i.IsGenericType)
                                                        .First(i => expectedOpenGenericTypes.Contains(i.GetGenericTypeDefinition()));
            var openGenericType = closedGenericType.GetGenericTypeDefinition();
            var eventType = closedGenericType.GetGenericArguments().Single();

            var eventTypes = GetEventTypeHierarchy(eventType);
            return eventTypes.Select(t => openGenericType.MakeGenericType(t));
        }

        private MethodInfo FindHandleMethod(Type messageType, Type handlerType)
        {
            lock (_handleMethodCacheLock)
            {
                MethodInfo handleMethod;
                if (false == _handleMethodCache.TryGetValue(handlerType.TypeHandle, out handleMethod))
                {
                    handleMethod = handlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                              .Where(m => m.Name == "Handle")
                                              .Where(
                                                  m =>
                                                  {
                                                      var parameters = m.GetParameters();
                                                      return parameters.Length == 1
                                                             && parameters[0].ParameterType == messageType;
                                                  }).SingleOrDefault();

                    _handleMethodCache.Add(handlerType.TypeHandle, handleMethod);
                }

                return handleMethod;
            }
        }

        private class HandlerDescriptor
        {
            public MethodInfo MethodInfo { get; set; }
            public object Handler { get; set; }
        }
    }
}
