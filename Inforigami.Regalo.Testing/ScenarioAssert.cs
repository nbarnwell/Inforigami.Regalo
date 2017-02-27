using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using NUnit.Framework;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioAssert<TEntity, THandler, TCommand> : ScenarioAssertBase<THandler, TCommand>, IScenarioAssert<TEntity, THandler, TCommand> 
        where TEntity : AggregateRoot, new()
    {
        private readonly TEntity _entity;
        private readonly TestingMessageHandlerContext<TEntity> _context;
        private readonly IEnumerable<IEvent> _expected;

        public ScenarioAssert(TEntity entity, THandler handler, TCommand command, TestingMessageHandlerContext<TEntity> context, IEnumerable<IEvent> expected) 
            : base(handler, command)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _context = context;
            _expected = expected;
        }

        public void Assert()
        {
            /*
             * Invoke the command on the handler and retrieve the actual
             * events from the repository and eventbus, and compare both with the
             * expected events list passed-in using Inforigami.Regalo.ObjectCompare.
             */

            InvokeHandler();

            var eventsStoredToEventStore = _context.GetGeneratedEvents();

            var comparer = ObjectComparerProvider.Create();

            ObjectComparisonResult result = comparer.AreEqual(_expected, eventsStoredToEventStore);
            if (!result.AreEqual)
            {
                var message = $"Actual events did not match expected events. {result.InequalityReason}";

                if (_expected.Any() && !eventsStoredToEventStore.Any())
                {
                    message +=
                        "\r\nCheck that your Scenario-based test is using the same IMesasageHandlerContext throughout, in case events are written to one assertions are performed against another.";
                }

                throw new AssertionException(message);
            }
        }
    }
}
