using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IThenSetter<TEntity, THandler, TCommand>
    {
        IScenarioAssert<TEntity, THandler, TCommand> Then(Func<TEntity, TCommand, IEnumerable<IEvent>> func);
        IScenarioExceptionAssert<TException, TEntity, THandler, TCommand> Throws<TException>() where TException : Exception;
    }
}
