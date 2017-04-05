using System;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioAssert<TEntity, THandler, TMessage>
        where TMessage : IMessage
    {
        void Assert();
    }
}
