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
            if (context == null) throw new ArgumentNullException("context");

            _handler = handler;
            _context = context;
        }

        [Obsolete("Use the overload that takes an entity instead.")]
        public IWhenSetter<TEntity, THandler> Given(ITestDataBuilder<TEntity> testDataBuilder)
        {
            var entity = testDataBuilder.Build();
            return Given(entity);
        }

        public IWhenSetter<TEntity, THandler> Given(TEntity entity)
        {
            if (entity != null)
            {
                _context.SaveAndPublishEvents(entity);
            }

            _context.ClearGeneratedEvents();
            return new ScenarioWhenSetter<TEntity, THandler>(entity, _handler, _context);
        }
    }
}
