using Inforigami.Regalo.Messaging;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class CommandProcessorTestDataBuilder : ITestDataBuilder<CommandProcessor>
    {
        public string CurrentDescription { get; }

        public CommandProcessor Build()
        {
            var consoleLogger = new ConsoleLogger();
            return new CommandProcessor(consoleLogger, new NoHandlerFoundStrategyFactory(consoleLogger));
        }
    }
}