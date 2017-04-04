using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IThenSetter<TEntity, THandler>
    {
        IScenarioAssert<TEntity, THandler> Then(Func<TEntity, IMessage, IEnumerable<IEvent>> func);
        IScenarioExceptionAssert<TException, TEntity, THandler> Throws<TException>() where TException : Exception;
    }
}
