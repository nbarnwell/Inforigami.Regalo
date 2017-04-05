using System;
using NUnit.Framework.Constraints;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public class ScenarioExceptionAssert<TException, TEntity, THandler, TMessage> 
        : ScenarioAssertBase<THandler, TMessage>
        , IScenarioExceptionAssert<TException, TEntity, THandler, TMessage> 
        where TEntity : AggregateRoot, new() 
        where TMessage : IMessage
        where TException : Exception
    {
        private readonly TMessage _message;
        private readonly TEntity _entity;
        private readonly TestingMessageHandlerContext<TEntity> _context;

        public ScenarioExceptionAssert(TEntity entity, TestingMessageHandlerContext<TEntity> context, THandler handler, TMessage message) 
            : base(handler, message)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (message == null) throw new ArgumentNullException(nameof(message));

            _entity = entity;
            _context = context;
            _message = message;
        }

        public void Assert()
        {
            Exception exception = null;
            try
            {
                if (_entity != null)
                {
                    using (var t = _context.OpenSession(_message))
                    {
                        t.SaveAndPublishEvents(_entity);
                    }
                }

                _context.ClearGeneratedEvents();

                InvokeHandler();
            }
            catch (Exception ex)
            {
                exception = ex.GetBaseException();
            }

            NUnit.Framework.Assert.That(
                (object)exception,
                (IResolveConstraint)new ExceptionTypeConstraint(typeof(TException)),
                "Expected exception was not thrown.",
                null);
        }
    }
}
