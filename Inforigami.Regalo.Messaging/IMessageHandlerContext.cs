using System;

namespace Inforigami.Regalo.Messaging
{
    public interface IMessageHandlerContext<TEntity>
    {
        //TEntity Get(Guid id);
        TEntity Get(Guid id, int version);
        void SaveAndPublishEvents(TEntity entity);
    }
}
