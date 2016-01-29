using System;

namespace Inforigami.Regalo.Core
{
    public class Conventions
    {
        private static bool _aggregatesMustImplementApplyMethods = false;
        private static Func<Type, Type> _findAggregateTypeForEventType = null;
        private static Func<object, Exception, bool> _eventHandlingExceptionFilter = null;

        public static bool AggregatesMustImplementApplyMethods { get { return _aggregatesMustImplementApplyMethods; } }
        public static Func<Type, Type> FindAggregateTypeForEventType { get { return _findAggregateTypeForEventType; } }

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
    }
}
