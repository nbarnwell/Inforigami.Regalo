using System;

namespace Inforigami.Regalo.Core
{
    public class WarnNoHandlerFoundStrategy : INoHandlerFoundStrategy
    {
        private readonly ILogger _logger;

        public WarnNoHandlerFoundStrategy(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        public void Invoke(object message)
        {
            _logger.Warn(this, "No handlers found for message type {0}", message.GetType());
        }
    }
}