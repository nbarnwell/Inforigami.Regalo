using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class EventBusTests
    {
        private ObjectEventHandler _objectEventHandler;
        private EventHandlerA _eventHandlerA;
        private EventHandlerB _eventHandlerB;
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _objectEventHandler = new ObjectEventHandler();
            _eventHandlerA = new EventHandlerA();
            _eventHandlerB = new EventHandlerB();
            _eventBus = new EventBus(new ConsoleLogger());

            Resolver.Configure(type => null, LocateAllEventHandlers, o => { });
            Conventions.SetRetryableEventHandlingExceptionFilter(null);
        }

        private IEnumerable<object> LocateAllEventHandlers(Type type)
        {
            return new object[] { _objectEventHandler, _eventHandlerA, _eventHandlerB }
                .Where(x => type.IsAssignableFrom(x.GetType()));
        }

        [TearDown]
        public void TearDown()
        {
            Resolver.Reset();
        }

        [Test]
        public void GivenAMessage_WhenAskedToPublish_ShouldTryToFindHandlersForMessageTypeHierarchy()
        {
            var expected = new[]
            {
                typeof(IEventHandler<object>),
                typeof(IEventHandler<Message>),
                typeof(IEventHandler<Event>),
                typeof(IEventHandler<SimpleEventBase>),
                typeof(IEventHandler<SimpleEvent>),
                typeof(IEventHandler<IEventHandlingSucceededEvent<object>>),
                typeof(IEventHandler<IEventHandlingSucceededEvent<Message>>),
                typeof(IEventHandler<IEventHandlingSucceededEvent<Event>>),
                typeof(IEventHandler<IEventHandlingSucceededEvent<SimpleEventBase>>),
                typeof(IEventHandler<IEventHandlingSucceededEvent<SimpleEvent>>)
            };

            var result = new List<Type>();
            Resolver.Reset();
            Resolver.Configure(
                type => null,
                type =>
                {
                    result.Add(type);
                    return LocateAllEventHandlers(type);
                }, 
                o => { });

            _eventBus.Publish(new SimpleEvent());

            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void GivenAMessage_WhenAskedToProcess_ShouldInvokeHandlersInCorrectSequence()
        {
            var expected = new[]
            {
                typeof(object),
                typeof(SimpleEventBase),
                typeof(SimpleEvent),
            };

            _eventBus.Publish(new SimpleEvent());

            _objectEventHandler.TargetsCalled.ToList().ForEach(Console.WriteLine);

            CollectionAssert.AreEqual(expected, _objectEventHandler.TargetsCalled);
        }

        [Test]
        public void GivenAMessageHandledMultipleHandlers_WhenAskedToPublish_ShouldInvokeAllCommandHandlersInCorrectSequence()
        {
            var processor = new EventBus(new NullLogger());

            processor.Publish(new EventHandledByMultipleHandlers());

            CollectionAssert.AreEqual(new[] { typeof(object) }, _objectEventHandler.TargetsCalled);
            CollectionAssert.AreEqual(new[] { typeof(EventHandledByMultipleHandlers) }, _eventHandlerA.TargetsCalled);
            CollectionAssert.AreEqual(new[] { typeof(EventHandledByMultipleHandlers) }, _eventHandlerB.TargetsCalled);
        }

        [Test]
        public void GivenAMessageThatWillFailHandling_WhenAskedToPublish_ShouldGenerateFailedHandlingMessage()
        {
            var failingEventHandler = new FailingEventHandler();
            Resolver.Reset();
            Resolver.Configure(
                type => null,
                type => new object[] { failingEventHandler }.Where(x => type.IsAssignableFrom(x.GetType())),
                o => { });

            var eventThatWillFailToBeHandled = new SimpleEvent();
            _eventBus.Publish(eventThatWillFailToBeHandled);

            CollectionAssert.AreEqual(
                new[]
                {
                    typeof(SimpleEvent),
                    typeof(IEventHandlingFailedEvent<SimpleEvent>)
                },
                failingEventHandler.TargetsCalled);
        }

        [Test]
        public void GivenAMessageThatWillFailHandling_WhenAskedToPublish_ShouldAllowRetryableExceptionsToPropagate()
        {
            Conventions.SetRetryableEventHandlingExceptionFilter((o, e) => true);
            var failingEventHandler = new FailingEventHandler();
            Resolver.Reset();
            Resolver.Configure(
                type => null,
                type => new object[] { failingEventHandler }.Where(x => type.IsAssignableFrom(x.GetType())),
                o => { });

            var eventThatWillFailToBeHandled = new SimpleEvent();
            var exception = Assert.Throws<TargetInvocationException>(() => _eventBus.Publish(eventThatWillFailToBeHandled));

            CollectionAssert.AreEqual(
                new[]
                {
                    typeof(SimpleEvent)
                },
                failingEventHandler.TargetsCalled);
        }
    }

    public class EventHandledByMultipleHandlers : Event
    {
    }

    public class EventHandlerA : IEventHandler<EventHandledByMultipleHandlers>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();

        public void Handle(EventHandledByMultipleHandlers evt)
        {
            TargetsCalled.Add(typeof(EventHandledByMultipleHandlers));
        }
    }

    public class EventHandlerB : IEventHandler<EventHandledByMultipleHandlers>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();

        public void Handle(EventHandledByMultipleHandlers evt)
        {
            TargetsCalled.Add(typeof(EventHandledByMultipleHandlers));
        }
    }

    public class SimpleEvent : SimpleEventBase
    {
    }

    public class SimpleEventBase : Event // Remember this inherits from object...
    {
    }

    public class ObjectEventHandler
        : IEventHandler<object>,
        IEventHandler<SimpleEventBase>,
        IEventHandler<SimpleEvent>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();
        public readonly IList<Type> MessageTypes = new List<Type>();

        public void Handle(object evt)
        {
            TargetsCalled.Add(typeof(object));
            MessageTypes.Add(evt.GetType());
        }

        void IEventHandler<SimpleEventBase>.Handle(SimpleEventBase evt)
        {
            TargetsCalled.Add(typeof(SimpleEventBase));
            MessageTypes.Add(evt.GetType());
        }

        public void Handle(SimpleEvent evt)
        {
            TargetsCalled.Add(typeof(SimpleEvent));
            MessageTypes.Add(evt.GetType());
        }
    }

    public class FailingEventHandler : IEventHandler<SimpleEvent>, IEventHandler<IEventHandlingFailedEvent<SimpleEvent>>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();
        public readonly IList<Type> MessageTypes = new List<Type>();

        public void Handle(SimpleEvent evt)
        {
            TargetsCalled.Add(typeof(SimpleEvent));
            MessageTypes.Add(evt.GetType());

            throw new Exception("Deliberate failure.");
        }

        public void Handle(IEventHandlingFailedEvent<SimpleEvent> evt)
        {
            TargetsCalled.Add(typeof(IEventHandlingFailedEvent<SimpleEvent>));
            MessageTypes.Add(evt.GetType());
        }
    }
}
