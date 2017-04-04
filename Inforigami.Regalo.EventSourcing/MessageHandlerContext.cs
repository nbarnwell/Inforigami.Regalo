using System;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.EventSourcing
{
    public class MessageHandlerContext<TEntity> : IMessageHandlerContext<TEntity> 
        where TEntity : AggregateRoot, new()
    {
        private readonly IRepository<TEntity> _repository;
        private readonly IEventBus _eventBus;

        public MessageHandlerContext(IRepository<TEntity> repository, IEventBus eventBus)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (eventBus == null) throw new ArgumentNullException("eventBus");

            _repository = repository;
            _eventBus = eventBus;
        }

        public IMessageHandlerContextToken<TEntity> OpenSession(IMessage currentMessage)
        {
            return new MessageHandlerContextToken<TEntity>(_repository, _eventBus, currentMessage);
        }
    }
}
