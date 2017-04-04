using System;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioGivenSetter<TEntity, THandler> : IGivenSetter<TEntity, THandler> 
        where TEntity : AggregateRoot, new()
    {
        private readonly THandler _handler;
        private readonly TestingMessageHandlerContext<TEntity> _context;

        public ScenarioGivenSetter(THandler handler, TestingMessageHandlerContext<TEntity> context)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (context == null) throw new ArgumentNullException("context");

            _handler = handler;
            _context = context;
        }

        public IWhenSetter<TEntity, THandler> Given(TEntity entity)
        {
            return new ScenarioWhenSetter<TEntity, THandler>(entity, _handler, _context);
        }
    }
}
