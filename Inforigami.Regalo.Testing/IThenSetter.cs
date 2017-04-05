using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IThenSetter<TEntity, THandler, TCommand>
        where TCommand : IMessage
    {
        IScenarioAssert<TEntity, THandler, TCommand> Then(Func<TEntity, TCommand, IEnumerable<IEvent>> expected);
        IScenarioExceptionAssert<TException, TEntity, THandler, TCommand> Throws<TException>() where TException : Exception;
    }
}
