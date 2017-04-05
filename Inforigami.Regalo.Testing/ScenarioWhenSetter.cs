using System;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioWhenSetter<TEntity, THandler> : IWhenSetter<TEntity, THandler> 
        where TEntity : AggregateRoot, new()
    {
        private readonly TEntity _entity;
        private readonly THandler _handler;
        private readonly TestingMessageHandlerContext<TEntity> _context;

        public ScenarioWhenSetter(TEntity entity, THandler handler, TestingMessageHandlerContext<TEntity> context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _handler = handler;
            _context = context;
        }

        public IThenSetter<TEntity, THandler, TMessage> When<TMessage>(Func<TEntity, TMessage> func)
            where TMessage : IMessage
        {
            var command = func.Invoke(_entity);
            return new ScenarioThenSetter<TEntity, THandler, TMessage>(_entity, _handler, _context, command);
        }
    }
}
