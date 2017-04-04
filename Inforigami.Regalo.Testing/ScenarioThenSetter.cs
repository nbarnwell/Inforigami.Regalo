using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioThenSetter<TEntity, THandler> : IThenSetter<TEntity, THandler> 
        where TEntity : AggregateRoot, new()
    {
        private readonly TEntity _entity;
        private readonly THandler _handler;
        private readonly TestingMessageHandlerContext<TEntity> _context;
        private readonly IMessage _command;

        public ScenarioThenSetter(TEntity entity, THandler handler, TestingMessageHandlerContext<TEntity> context, IMessage command)
        {
            if (context == null) throw new ArgumentNullException("context");

            _entity = entity;
            _handler = handler;
            _context = context;
            _command = command;
        }

        public IScenarioAssert<TEntity, THandler> Then(Func<TEntity, IMessage, IEnumerable<IEvent>> expected)
        {
            return new ScenarioAssert<TEntity, THandler>(_entity, _handler, _command, _context, expected);
        }

        public IScenarioExceptionAssert<TException, TEntity, THandler> Throws<TException>() where TException : Exception
        {
            return new ScenarioExceptionAssert<TException, TEntity, THandler>(_entity, _context, _handler, _command);
        }
    }
}
