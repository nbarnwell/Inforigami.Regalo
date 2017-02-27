using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.Testing
{
    public class TestingMessageHandlerContext<TEntity> : IMessageHandlerContext<TEntity> 
        where TEntity : AggregateRoot, new()
    {
        private readonly IList<object>        _events = new List<object>();
        private readonly IRepository<TEntity> _repository;
        private readonly IEventBus            _eventBus;

        public TestingMessageHandlerContext(IRepository<TEntity> repository, IEventBus eventBus)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (eventBus == null)   throw new ArgumentNullException("eventBus");

            _repository = repository;
            _eventBus   = eventBus;
        }

        //public TEntity Get(Guid id)
        //{
        //    return _repository.Get(id.ToString());
        //}

        public TEntity Get(Guid id, int version)
        {
            return _repository.Get(id, version);
        }

        public void SaveAndPublishEvents(TEntity entity)
        {
            var uncommittedEvents = entity.GetUncommittedEvents();
            _repository.Save(entity);
            _eventBus.Publish(uncommittedEvents);

            foreach (var evt in uncommittedEvents)
            {
                _events.Add(evt);
            }
        }

        public IEnumerable<object> GetGeneratedEvents()
        {
            return _events;
        }

        public void ClearGeneratedEvents()
        {
            _events.Clear();
        }
    }
}
