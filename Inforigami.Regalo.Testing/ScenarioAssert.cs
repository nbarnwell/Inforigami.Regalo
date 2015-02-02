using System;
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
        private readonly object[] _expected;

        public ScenarioAssert(TEntity entity, THandler handler, TCommand command, TestingMessageHandlerContext<TEntity> context, object[] expected) 
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
             * Plan is to invoke the command on the handler and retrieve the actual
             * events from the repository and eventbus, and compare both with the
             * expected events list passed-in using Inforigami.Regalo.ObjectCompare.
             */

            InvokeHandler();

            var eventsStoredToEventStore = _context.GetGeneratedEvents();

            var comparer = new ObjectComparer().Ignore<IEvent, Guid>(x => x.Id)
                                               .Ignore<IEvent, Guid>(x => x.CausationId)
                                               .Ignore<IEvent, Guid>(x => x.CorrelationId);

            ObjectComparisonResult result = comparer.AreEqual(_expected, eventsStoredToEventStore);
            if (!result.AreEqual)
            {
                throw new AssertionException(string.Format("Actual events did not match expected events. {0}", result.InequalityReason));
            }
        }
    }
}
