using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public class NoHandlerFoundStrategyFactory : INoHandlerFoundStrategyFactory
    {
        private readonly ILogger _logger;

        private readonly object _cacheLock = new object();
        private readonly IDictionary<CacheKey, bool> _cache = new Dictionary<CacheKey, bool>();

        public NoHandlerFoundStrategyFactory(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        public INoHandlerFoundStrategy Create<TMessage>(TMessage message)
        {
            var messageType = message.GetType();

            if (IsMessageHandlingFailedEvent(messageType))
            {
                return CreateStrategy(Conventions.BehaviourWhenNoFailedMessageHandlerFound);
            }
            else if (IsMessageHandlingSuccessEvent(messageType))
            {
                return CreateStrategy(Conventions.BehaviourWhenNoSuccessMessageHandlerFound);
            }
            else
            {
                return CreateStrategy(Conventions.BehaviourWhenNoMessageHandlerFound);
            }
        }

        private INoHandlerFoundStrategy CreateStrategy(NoMessageHandlerBehaviour behaviour)
        {
            switch (behaviour)
            {
                case NoMessageHandlerBehaviour.Ignore:
                    return new IgnoreNoHandlerFoundStrategy(_logger);
                case NoMessageHandlerBehaviour.Throw:
                    return new ThrowExceptionNoHandlerFoundStrategy();
                case NoMessageHandlerBehaviour.Warn:
                default:
                    return new WarnNoHandlerFoundStrategy(_logger);
            }
        }

        private bool IsMessageHandlingSuccessEvent(Type messageType)
        {
            return IsMessageHandlingWrapperEvent(messageType, typeof(IEventHandlingSucceededEvent<>));
        }

        private bool IsMessageHandlingFailedEvent(Type messageType)
        {
            return IsMessageHandlingWrapperEvent(messageType, typeof(IEventHandlingFailedEvent<>));
        }

        private bool IsMessageHandlingWrapperEvent(Type messageType, Type wrapperMessageType)
        {
            lock (_cacheLock)
            {
                var key = new CacheKey(messageType.TypeHandle, wrapperMessageType.TypeHandle);

                bool result;
                if (!_cache.TryGetValue(key, out result))
                {
                    result = messageType.GetInterfaces()
                                        .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == wrapperMessageType);
                    _cache.Add(key, result);
                }

                return result;
            }
        }

        private class CacheKey
        {
            public RuntimeTypeHandle MessageType { get; }
            public RuntimeTypeHandle WrapperType { get; }

            public CacheKey(RuntimeTypeHandle messageType, RuntimeTypeHandle wrapperType)
            {
                MessageType = messageType;
                WrapperType = wrapperType;
            }

            protected bool Equals(CacheKey other)
            {
                return MessageType.Equals(other.MessageType) && WrapperType.Equals(other.WrapperType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked { return (MessageType.GetHashCode() * 397) ^ WrapperType.GetHashCode(); }
            }

            private sealed class MessageTypeWrapperTypeEqualityComparer : IEqualityComparer<CacheKey>
            {
                public bool Equals(CacheKey x, CacheKey y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return x.MessageType.Equals(y.MessageType) && x.WrapperType.Equals(y.WrapperType);
                }

                public int GetHashCode(CacheKey obj)
                {
                    unchecked { return (obj.MessageType.GetHashCode() * 397) ^ obj.WrapperType.GetHashCode(); }
                }
            }

            private static readonly IEqualityComparer<CacheKey> MessageTypeWrapperTypeComparerInstance = new MessageTypeWrapperTypeEqualityComparer();

            public static IEqualityComparer<CacheKey> MessageTypeWrapperTypeComparer
            {
                get { return MessageTypeWrapperTypeComparerInstance; }
            }
        }
    }
}