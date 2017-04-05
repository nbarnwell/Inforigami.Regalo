using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IWhenSetter<TEntity, THandler>
    {
        IThenSetter<TEntity, THandler, TMessage> When<TMessage>(Func<TEntity, TMessage> func)
            where TMessage : IMessage;
    }
}
