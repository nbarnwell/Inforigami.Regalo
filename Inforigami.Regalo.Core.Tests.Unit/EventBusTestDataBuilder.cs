using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class EventBusTestDataBuilder : ITestDataBuilder<EventBus>
    {
        public string CurrentDescription { get; }
        public EventBus Build()
        {
            var logger = new ConsoleLogger();
            return new EventBus(logger, new NoHandlerFoundStrategyFactory(logger));
        }
    }
}