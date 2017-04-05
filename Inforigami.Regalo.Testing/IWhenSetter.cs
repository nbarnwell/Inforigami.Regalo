using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IWhenSetter<TEntity, THandler>
    {
        IThenSetter<TEntity, THandler, TCommand> When<TCommand>(Func<TEntity, TCommand> func)
            where TCommand : IMessage;
    }
}
