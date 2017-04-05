using System;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioAssert<TEntity, THandler, TCommand>
        where TCommand : IMessage
    {
        void Assert();
    }
}
