using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IMessageHandlerContext<TEntity> 
        where TEntity : AggregateRoot, new()
    {
        IMessageHandlerContextToken<TEntity> OpenSession(IMessage currentMessage);
    }
}
