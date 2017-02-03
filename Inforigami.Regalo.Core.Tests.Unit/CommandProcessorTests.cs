using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class CommandProcessorTests
    {
        private ObjectCommandHandler _objectCommandHandler;
        private CommandHandlerA _commandHandlerA;
        private CommandHandlerB _commandHandlerB;
            
        [SetUp]
        public void SetUp()
        {
            _objectCommandHandler = new ObjectCommandHandler();
            _commandHandlerA = new CommandHandlerA();
            _commandHandlerB = new CommandHandlerB();
            Resolver.Configure(type => null, LocateAllCommandHandlers, o => { });
        }

        private IEnumerable<object> LocateAllCommandHandlers(Type type)
        {
            return new object[] { _objectCommandHandler, _commandHandlerA, _commandHandlerB }
                .Where(x => type.IsAssignableFrom(x.GetType()));
        }

        [TearDown]
        public void TearDown()
        {
            Resolver.Reset();
        }

        [Test]
        public void GivenAMessage_WhenAskedToProcess_ShouldTryToFindHandlersForMessageTypeHierarchy()
        {
            var expected = new[]
            {
                typeof(ICommandHandler<object>),
                typeof(ICommandHandler<SimpleCommandBase>),
                typeof(ICommandHandler<SimpleCommand>),
            };

            var result = new List<Type>();
            Resolver.Reset();
            Resolver.Configure(
                type => null,
                type =>
                {
                    result.Add(type);
                    return LocateAllCommandHandlers(type);
                },
                o => { });

            var processor = new CommandProcessorTestDataBuilder().Build();

            processor.Process(new SimpleCommand());

            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void GivenAMessage_WhenAskedToProcess_ShouldInvokeCommandHandlersInCorrectSequence()
        {
            var expected = new[]
            {
                typeof(object),
                typeof(SimpleCommandBase),
                typeof(SimpleCommand),
            };

            var processor = new CommandProcessorTestDataBuilder().Build();

            processor.Process(new SimpleCommand());

            _objectCommandHandler.Messages.ToList().ForEach(Console.WriteLine);

            CollectionAssert.AreEqual(expected, _objectCommandHandler.Messages);
        }

        [Test]
        public void GivenAMessageHandledMultipleHandlers_WhenAskedToProcess_ShouldInvokeAllCommandHandlersInCorrectSequence()
        {
            var processor = new CommandProcessorTestDataBuilder().Build();

            processor.Process(new CommandHandledByMultipleHandlers());

            CollectionAssert.AreEqual(new [] { typeof(object) }, _objectCommandHandler.Messages);
            CollectionAssert.AreEqual(new [] { typeof(CommandHandledByMultipleHandlers) }, _commandHandlerA.Messages);
            CollectionAssert.AreEqual(new [] { typeof(CommandHandledByMultipleHandlers) }, _commandHandlerB.Messages);
        }
    }
}
