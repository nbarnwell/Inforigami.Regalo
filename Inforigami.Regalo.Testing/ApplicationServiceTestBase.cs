using NUnit.Framework;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Testing
{
    public class ApplicationServiceTestBase<TEntity> 
        where TEntity : AggregateRoot, new()
    {
        protected TestingMessageHandlerContext<TEntity> Context { get; set; } 

        [SetUp]
        public void SetUp()
        {
            var eventStore = new InMemoryEventStore(new ConsoleLogger());
            var repository = new EventSourcingRepository<TEntity>(eventStore, new StrictConcurrencyMonitor(), new ConsoleLogger());
            var eventBus = new FakeEventBus();
            Context = new TestingMessageHandlerContext<TEntity>(repository, eventBus);
        }

        [TearDown]
        public void TearDown()
        {
            Context = null;
        }
    }
}
