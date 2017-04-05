using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioThenSetter<TEntity, THandler, TMessage> : IThenSetter<TEntity, THandler, TMessage> 
        where TEntity : AggregateRoot, new()
        where TMessage : IMessage
    {
        private readonly TEntity _entity;
        private readonly THandler _handler;
        private readonly TestingMessageHandlerContext<TEntity> _context;
        private readonly TMessage _message;

        public ScenarioThenSetter(TEntity entity, THandler handler, TestingMessageHandlerContext<TEntity> context, TMessage message)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _handler = handler;
            _context = context;
            _message = message;
        }

        public IScenarioAssert<TEntity, THandler, TMessage> Then(Func<TEntity, TMessage, IEnumerable<IEvent>> expected)
        {
            return new ScenarioAssert<TEntity, THandler, TMessage>(_entity, _handler, _message, _context, expected);
        }

        public IScenarioExceptionAssert<TException, TEntity, THandler, TMessage> Throws<TException>() where TException : Exception
        {
            return new ScenarioExceptionAssert<TException, TEntity, THandler, TMessage>(_entity, _context, _handler, _message);
        }
    }
}
