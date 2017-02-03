using System;

namespace Inforigami.Regalo.Core
{
    public interface INoHandlerFoundStrategyFactory
    {
        INoHandlerFoundStrategy Create<TMessage>(TMessage message);
    }
}