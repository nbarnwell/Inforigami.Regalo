using System;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Testing
{
    public interface IGivenSetter<TEntity, THandler>
        where TEntity : AggregateRoot, new()
    {
        IWhenSetter<TEntity, THandler> Given(TEntity entity);
    }
}
