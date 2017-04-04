using System;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.EventSourcing
{
    public class MessageHandlerContextToken<TEntity> : IMessageHandlerContextToken<TEntity>
        where TEntity : AggregateRoot, new()
    {
        private bool _isDisposed;
        private IRepository<TEntity> _repository;
        private IEventBus _eventBus;
        private IMessage _currentMessage;

        public MessageHandlerContextToken(IRepository<TEntity> repository, IEventBus eventBus, IMessage currentMessage)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (eventBus == null) throw new ArgumentNullException(nameof(eventBus));
            if (currentMessage == null) throw new ArgumentNullException(nameof(currentMessage));

            _repository = repository;
            _eventBus = eventBus;
            _currentMessage = currentMessage;
        }

        public TEntity Get(Guid id, int version)
        {
            return _repository.Get(id, version);
        }

        public void SaveAndPublishEvents(TEntity entity)
        {
            var uncommittedEvents = entity.GetUncommittedEvents();

            foreach (var message in uncommittedEvents)
            {
                message.WasCausedBy(_currentMessage);
            }

            _repository.Save(entity);
            _eventBus.Publish(uncommittedEvents);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentMessage = null;
                _repository = null;
                _eventBus = null;
                _isDisposed = true;
            }
        }
    }
}