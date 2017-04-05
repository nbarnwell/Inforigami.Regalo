using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
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

        public IEnumerable<object> GetGeneratedEvents()
        {
            return _events;
        }

        public void ClearGeneratedEvents()
        {
            _events.Clear();
        }

        public IMessageHandlerContextSession<TEntity> OpenSession(IMessage currentMessage)
        {
            return new TestMessageHandlerContextSession<TEntity>(_repository, _eventBus, currentMessage, this);
        }

        public void AddEvent(IEvent evt)
        {
            _events.Add(evt);
        }
    }

    public class TestMessageHandlerContextSession<TEntity> : IMessageHandlerContextSession<TEntity>
        where TEntity : AggregateRoot, new()
    {
        private readonly IRepository<TEntity> _repository;
        private readonly IEventBus _eventBus;
        private readonly IMessage _currentMessage;
        private readonly TestingMessageHandlerContext<TEntity> _context;

        public TestMessageHandlerContextSession(IRepository<TEntity> repository, IEventBus eventBus, IMessage currentMessage, TestingMessageHandlerContext<TEntity> context)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));
            if (currentMessage == null) throw new ArgumentNullException(nameof(currentMessage));
            if (context == null) throw new ArgumentNullException(nameof(context));

            _repository = repository;
            _eventBus = eventBus;
            _currentMessage = currentMessage;
            _context = context;
        }

        public TEntity Get(Guid id, int version)
        {
            return _repository.Get(id, version);
        }

        public void SaveAndPublishEvents(TEntity entity)
        {
            var uncommittedEvents = entity.GetUncommittedEvents();

            foreach (var evt in uncommittedEvents)
            {
                evt.WasCausedBy(_currentMessage);
            }

            _repository.Save(entity);
            _eventBus.Publish(uncommittedEvents);

            foreach (var evt in uncommittedEvents)
            {
                _context.AddEvent(evt);
            }
        }

        public void Dispose()
        {
        }
    }
}
