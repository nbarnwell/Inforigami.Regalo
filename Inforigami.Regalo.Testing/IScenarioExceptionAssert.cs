using System;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioExceptionAssert<TException, TEntity, THandler>
        where TException : Exception
    {
        void Assert();
    }
}
