using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Messaging;
using NUnit.Framework;

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
            Conventions.ResetToDefaults();
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
        public void GivenAMessageHandledByMultipleHandlers_WhenAskedToProcess_ShouldInvokeAllCommandHandlersInCorrectSequence()
        {
            var processor = new CommandProcessorTestDataBuilder().Build();

            var command = new CommandHandledByMultipleHandlers();
            processor.Process(command);

            var expected =
                new[]
                {
                    typeof(ObjectCommandHandler),
                    typeof(CommandHandlerA),
                    typeof(CommandHandlerB)
                };

            CollectionAssert.AreEqual(
                expected,
                command.HandlersThatHandledThisMessage
                       .Select(x => x.GetType())
                       .ToArray());
        }

        [Test]
        public void GivenAMessageHandledByMultipleHandlers_WhenAskedToProcessWithDifferentHandlerOrdering_ShouldInvokeAllCommandHandlersInCorrectSequence()
        {
            Conventions.SetHandlerSortingMethod((x, y) => string.Compare(y.GetType().FullName, x.GetType().FullName, StringComparison.InvariantCultureIgnoreCase));

            var processor = new CommandProcessorTestDataBuilder().Build();

            var command = new CommandHandledByMultipleHandlers();
            processor.Process(command);

            var expected =
                new[]
                {
                    typeof(ObjectCommandHandler),
                    typeof(CommandHandlerB),
                    typeof(CommandHandlerA)
                };

            CollectionAssert.AreEqual(
                expected,
                command.HandlersThatHandledThisMessage
                       .Select(x => x.GetType())
                       .ToArray());
        }
    }
}
