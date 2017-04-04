using System;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioAssert<TEntity, THandler>
    {
        void Assert();
    }
}
