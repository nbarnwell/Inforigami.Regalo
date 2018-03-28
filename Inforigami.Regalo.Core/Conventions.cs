using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public class Conventions
    {
        private static bool _aggregatesMustImplementApplyMethods;
        private static Func<Type, Type> _findAggregateTypeForEventType;
        private static Func<object, Exception, bool> _eventHandlingExceptionFilter;
        private static NoMessageHandlerBehaviour _behaviourWhenNoMessageHandlerFound;
        private static NoMessageHandlerBehaviour _behaviourWhenNoSuccessMessageHandlerFound;
        private static NoMessageHandlerBehaviour _behaviourWhenNoFailedMessageHandlerFound;
        private static Comparison<object> _handlerSortingMethod;
        private static Func<IEvent, string> _eventDescriptor;

        public static bool AggregatesMustImplementApplyMethods => _aggregatesMustImplementApplyMethods;
        public static Func<Type, Type> FindAggregateTypeForEventType => _findAggregateTypeForEventType;
        public static NoMessageHandlerBehaviour BehaviourWhenNoMessageHandlerFound => _behaviourWhenNoMessageHandlerFound;
        public static NoMessageHandlerBehaviour BehaviourWhenNoSuccessMessageHandlerFound => _behaviourWhenNoSuccessMessageHandlerFound;
        public static NoMessageHandlerBehaviour BehaviourWhenNoFailedMessageHandlerFound => _behaviourWhenNoFailedMessageHandlerFound;
        public static Comparison<object> HandlerSortingMethod => _handlerSortingMethod;
        public static Func<IEvent, string> EventDescriptor => _eventDescriptor;

        public static string StreamIdFormat
        {
            get { return "${aggregateTypeName}-${aggregateId}"; }
        }

        /// <summary>
        /// If returns true, exception will be "bubbled" to the caller. If false, the framework will attempt to 
        /// publish an EventHandlingFailedEvent that should be handled by an appropriate handler.
        /// </summary>
        public static Func<object, Exception, bool> EventPublishingExceptionFilter { get { return _eventHandlingExceptionFilter; } }

        static Conventions()
        {
            ResetToDefaults();
        }

        public static void ResetToDefaults()
        {
            _aggregatesMustImplementApplyMethods = false;
            _findAggregateTypeForEventType = null;
            _eventHandlingExceptionFilter = null;
            _behaviourWhenNoMessageHandlerFound = NoMessageHandlerBehaviour.Throw;
            _behaviourWhenNoSuccessMessageHandlerFound = NoMessageHandlerBehaviour.Ignore;
            _behaviourWhenNoFailedMessageHandlerFound = NoMessageHandlerBehaviour.Warn;
            _handlerSortingMethod =
                (x, y) =>
                    string.Compare(
                        x.GetType().FullName,
                        y.GetType().FullName,
                        StringComparison.InvariantCultureIgnoreCase);

            _eventDescriptor = evt => evt.GetType().ToString() + " (Id " + evt.MessageId + ", Ver " + EntityVersion.GetName(evt.Version) + ")";
        }

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

        public static void SetHandlerSortingMethod(Comparison<object> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));

            _handlerSortingMethod = comparison;
        }

        public static void SetEventDescriptor(Func<IEvent, string> descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

            _eventDescriptor = descriptor;
        }
    }

    public enum NoMessageHandlerBehaviour
    {
        Throw,
        Warn,
        Ignore
    }
}
