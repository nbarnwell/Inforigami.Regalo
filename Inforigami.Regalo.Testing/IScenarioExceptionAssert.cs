using System;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioExceptionAssert<TException, TEntity, THandler, TCommand>
        where TException : Exception
    {
        void Assert();
    }
}
