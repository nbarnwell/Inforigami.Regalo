using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public interface IThenSetter<TEntity, THandler, TMessage>
        where TMessage : IMessage
    {
        IScenarioAssert<TEntity, THandler, TMessage> Then(Func<TEntity, TMessage, IEnumerable<IEvent>> expected);
        IScenarioExceptionAssert<TException, TEntity, THandler, TMessage> Throws<TException>() where TException : Exception;
    }
}
