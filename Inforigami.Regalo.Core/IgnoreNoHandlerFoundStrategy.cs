using System;

namespace Inforigami.Regalo.Core
{
    public class IgnoreNoHandlerFoundStrategy : INoHandlerFoundStrategy
    {
        private readonly ILogger _logger;

        public IgnoreNoHandlerFoundStrategy(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        public void Invoke(object message)
        {
            _logger.Debug(this, "No handlers found for message type {0}", message.GetType());
        }
    }
}