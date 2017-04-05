using System;

namespace Inforigami.Regalo.Testing
{
    public interface IScenarioExceptionAssert<TException, TEntity, THandler, TMessage>
        where TException : Exception
    {
        void Assert();
    }
}
