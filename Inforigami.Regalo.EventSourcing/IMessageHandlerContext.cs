using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IMessageHandlerContext<TEntity> 
        where TEntity : AggregateRoot, new()
    {
        IMessageHandlerContextSession<TEntity> OpenSession(IMessage currentMessage);
    }
}
