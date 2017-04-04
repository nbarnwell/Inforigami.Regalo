using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IWhenSetter<TEntity, THandler>
    {
        IThenSetter<TEntity, THandler> When(Func<TEntity, IMessage> func);
    }
}
