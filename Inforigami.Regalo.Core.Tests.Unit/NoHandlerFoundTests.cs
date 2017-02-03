using System;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Testing;
using NUnit.Framework;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class NoHandlerFoundTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetConventions();
        }

        [TearDown]
        public void TearDown()
        {
            ResetConventions();
        }

        private void ResetConventions()
        {
            Conventions.SetBehaviourWhenNoEventHandlerFound(NoMessageHandlerBehaviour.Throw);
            Conventions.SetBehaviourWhenNoFailedEventHandlerFound(NoMessageHandlerBehaviour.Warn);
            Conventions.SetBehaviourWhenNoSuccessEventHandlerFound(NoMessageHandlerBehaviour.Ignore);
        }

        [Test]
        [TestCase(NoMessageHandlerBehaviour.Throw, typeof(ThrowExceptionNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Ignore, typeof(IgnoreNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Warn, typeof(WarnNoHandlerFoundStrategy))]
        public void NoHandlerFoundForStandardMessage(NoMessageHandlerBehaviour behaviour, Type expectedStrategyType)
        {
            var logger = new ConsoleLogger();
            var factory = new NoHandlerFoundStrategyFactory(logger);
            Conventions.SetBehaviourWhenNoEventHandlerFound(behaviour);

            var strategy = factory.Create(new object());

            Assert.That(strategy, Is.InstanceOf(expectedStrategyType));
        }

        [Test]
        [TestCase(NoMessageHandlerBehaviour.Throw, typeof(ThrowExceptionNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Ignore, typeof(IgnoreNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Warn, typeof(WarnNoHandlerFoundStrategy))]
        public void NoHandlerFoundForSuccessWrapperMessage(NoMessageHandlerBehaviour behaviour, Type expectedStrategyType)
        {
            var logger = new ConsoleLogger();
            var factory = new NoHandlerFoundStrategyFactory(logger);
            Conventions.SetBehaviourWhenNoSuccessEventHandlerFound(behaviour);

            var strategy = factory.Create(new EventHandlingSucceededEvent<object>(new object()));

            Assert.That(strategy, Is.InstanceOf(expectedStrategyType));
        }

        [Test]
        [TestCase(NoMessageHandlerBehaviour.Throw, typeof(ThrowExceptionNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Ignore, typeof(IgnoreNoHandlerFoundStrategy))]
        [TestCase(NoMessageHandlerBehaviour.Warn, typeof(WarnNoHandlerFoundStrategy))]
        public void NoHandlerFoundForFailedWrapperMessage(NoMessageHandlerBehaviour behaviour, Type expectedStrategyType)
        {
            var logger = new ConsoleLogger();
            var factory = new NoHandlerFoundStrategyFactory(logger);
            Conventions.SetBehaviourWhenNoFailedEventHandlerFound(behaviour);

            var strategy = factory.Create(new EventHandlingFailedEvent<object>(new object(), new Exception()));

            Assert.That(strategy, Is.InstanceOf(expectedStrategyType));
        }
    }
}