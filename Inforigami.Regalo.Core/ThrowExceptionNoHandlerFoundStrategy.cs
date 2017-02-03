using System;

namespace Inforigami.Regalo.Core
{
    public class ThrowExceptionNoHandlerFoundStrategy : INoHandlerFoundStrategy
    {
        public void Invoke(object message)
        {
            throw new InvalidOperationException($"No handlers found for message type {message.GetType()}");
        }
    }
}