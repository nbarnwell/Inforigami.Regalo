using System;

namespace Inforigami.Regalo.Core
{
    public interface IMessageHandlerContext<TEntity>
    {
        //TEntity Get(Guid id);
        TEntity Get(Guid id, int version);
        void SaveAndPublishEvents(TEntity entity);
    }
}
