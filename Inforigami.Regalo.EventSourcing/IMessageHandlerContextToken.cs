using System;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IMessageHandlerContextToken<TEntity> : IDisposable
        where TEntity : AggregateRoot, new()
    {
        TEntity Get(Guid id, int version);
        void SaveAndPublishEvents(TEntity entity);
    }
}