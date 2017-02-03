using System;

namespace Inforigami.Regalo.Core
{
    public class Conventions
    {
        private static bool _aggregatesMustImplementApplyMethods = false;
        private static Func<Type, Type> _findAggregateTypeForEventType = null;
        private static Func<object, Exception, bool> _eventHandlingExceptionFilter = null;
        private static NoMessageHandlerBehaviour _behaviourWhenNoMessageHandlerFound = NoMessageHandlerBehaviour.Throw;
        private static NoMessageHandlerBehaviour _behaviourWhenNoSuccessMessageHandlerFound = NoMessageHandlerBehaviour.Ignore;
        private static NoMessageHandlerBehaviour _behaviourWhenNoFailedMessageHandlerFound = NoMessageHandlerBehaviour.Warn;

        public static bool AggregatesMustImplementApplyMethods { get { return _aggregatesMustImplementApplyMethods; } }
        public static Func<Type, Type> FindAggregateTypeForEventType { get { return _findAggregateTypeForEventType; } }
        public static NoMessageHandlerBehaviour BehaviourWhenNoMessageHandlerFound { get { return _behaviourWhenNoMessageHandlerFound;} }
        public static NoMessageHandlerBehaviour BehaviourWhenNoSuccessMessageHandlerFound { get { return _behaviourWhenNoSuccessMessageHandlerFound;} }
        public static NoMessageHandlerBehaviour BehaviourWhenNoFailedMessageHandlerFound { get { return _behaviourWhenNoFailedMessageHandlerFound;} }

        public static string StreamIdFormat
        {
            get { return "${aggregateTypeName}-${aggregateId}"; }
        }

        /// <summary>
        /// If returns true, exception will be "bubbled" to the caller. If false, the framework will attempt to 
        /// publish an EventHandlingFailedEvent that should be handled by an appropriate handler.
        /// </summary>
        public static Func<object, Exception, bool> EventPublishingExceptionFilter { get { return _eventHandlingExceptionFilter; } }

        public static void SetAggregatesMustImplementApplymethods(bool value)
        {
            _aggregatesMustImplementApplyMethods = value;
        }

        public static void SetFindAggregateTypeForEventType(Func<Type, Type> findAggregateTypeForEventType)
        {
            _findAggregateTypeForEventType = findAggregateTypeForEventType;
        }

        public static void SetRetryableEventHandlingExceptionFilter(Func<object, Exception, bool> retryableEventHandlingExceptionFilter)
        {
            _eventHandlingExceptionFilter = retryableEventHandlingExceptionFilter;
        }

        public static void SetBehaviourWhenNoEventHandlerFound(NoMessageHandlerBehaviour newBehaviour)
        {
            _behaviourWhenNoMessageHandlerFound = newBehaviour;
        }

        public static void SetBehaviourWhenNoSuccessEventHandlerFound(NoMessageHandlerBehaviour newBehaviour)
        {
            _behaviourWhenNoSuccessMessageHandlerFound = newBehaviour;
        }

        public static void SetBehaviourWhenNoFailedEventHandlerFound(NoMessageHandlerBehaviour newBehaviour)
        {
            _behaviourWhenNoFailedMessageHandlerFound = newBehaviour;
        }
    }

    public enum NoMessageHandlerBehaviour
    {
        Throw,
        Warn,
        Ignore
    }
}
