using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
using NUnit.Framework;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioAssert<TEntity, THandler, TCommand> : ScenarioAssertBase<THandler, TCommand>, IScenarioAssert<TEntity, THandler, TCommand> 
        where TEntity : AggregateRoot, new()
        where TCommand : IMessage
    {
        private readonly TEntity _entity;
        private readonly TCommand _message;
        private readonly TestingMessageHandlerContext<TEntity> _context;
        private readonly Func<TEntity, TCommand, IEnumerable<IEvent>> _expected;

        public ScenarioAssert(TEntity entity, THandler handler, TCommand message, TestingMessageHandlerContext<TEntity> context, Func<TEntity, TCommand, IEnumerable<IEvent>> expected) 
            : base(handler, message)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _message = message;
            _context = context;
            _expected = expected;
        }

        public void Assert()
        {
            if (_entity != null)
            {
                using (var t = _context.OpenSession(_message))
                {
                    t.SaveAndPublishEvents(_entity);
                }
            }

            _context.ClearGeneratedEvents();

            var expected = _expected(_entity, _message);

            /*
             * Invoke the message on the handler and retrieve the actual
             * events from the repository and eventbus, and compare both with the
             * expected events list passed-in using Inforigami.Regalo.ObjectCompare.
             */
            InvokeHandler();

            var eventsStoredToEventStore = _context.GetGeneratedEvents();

            var comparer = ObjectComparerProvider.Create();

            ObjectComparisonResult result = comparer.AreEqual(expected, eventsStoredToEventStore);
            if (!result.AreEqual)
            {
                var message = $"Actual events did not match expected events. {result.InequalityReason}";

                if (expected.Any() && !eventsStoredToEventStore.Any())
                {
                    message +=
                        "\r\nCheck that your Scenario-based test is using the same IMesasageHandlerContext throughout, in case events are written to one assertions are performed against another.";
                }

                throw new AssertionException(message);
            }
        }
    }
}
