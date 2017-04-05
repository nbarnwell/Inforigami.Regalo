using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioThenSetter<TEntity, THandler, TCommand> : IThenSetter<TEntity, THandler, TCommand> 
        where TEntity : AggregateRoot, new()
        where TCommand : IMessage
    {
        private readonly TEntity _entity;
        private readonly THandler _handler;
        private readonly TestingMessageHandlerContext<TEntity> _context;
        private readonly TCommand _command;

        public ScenarioThenSetter(TEntity entity, THandler handler, TestingMessageHandlerContext<TEntity> context, TCommand command)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _handler = handler;
            _context = context;
            _command = command;
        }

        public IScenarioAssert<TEntity, THandler, TCommand> Then(Func<TEntity, TCommand, IEnumerable<IEvent>> expected)
        {
            return new ScenarioAssert<TEntity, THandler, TCommand>(_entity, _handler, _command, _context, expected);
        }

        public IScenarioExceptionAssert<TException, TEntity, THandler, TCommand> Throws<TException>() where TException : Exception
        {
            return new ScenarioExceptionAssert<TException, TEntity, THandler, TCommand>(_entity, _context, _handler, _command);
        }
    }
}
