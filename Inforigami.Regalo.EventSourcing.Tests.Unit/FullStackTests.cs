using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders;
using Inforigami.Regalo.RavenDB;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.TestDriver;

namespace Inforigami.Regalo.EventSourcing.Tests.Unit
{
    [TestFixtureSource(nameof(GetEventStore))]
    public class FullStackTests
    {
        private readonly IEventStore _eventStore;

        public FullStackTests(IEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        [SetUp]
        public void SetUp()
        {
            Resolver.Configure(
                type =>
                {
                    if (type == typeof(ILogger))
                    {
                        return new ConsoleLogger();
                    }

                    throw new NotSupportedException(string.Format("TestFixtureBase::SetUp - Nothing registered for {0}", type));
                },
                type => null,
                o => { });
        }

        [TearDown]
        public void TearDown()
        {
            Resolver.Reset();
        }

        [Test]
        public void Store_and_reload()
        {
            var order = CreateSalesOrder();
            AddLineItems(order.Id);
            PlaceOrder(order.Id);

            var repository = CreateRepository();
            var result     = repository.Get(order.Id, EntityVersion.Latest);

            Assert.AreEqual(order.Id, result.Id);
            Assert.IsTrue(result.IsPlaced, "Order.IsPlaced should be true");

            for (int i = 0; i < 15; i++)
            {
                Assert.AreEqual(1, result.Products[$"sku{i}"], $"Order should have quantity of 1 for 'sku{i}'");
            }
        }

        private void PlaceOrder(Guid orderId)
        {
            var repository = CreateRepository();
            var order      = repository.Get(orderId, EntityVersion.Latest);

            order.PlaceOrder();

            repository.Save(order);
        }

        private void AddLineItems(Guid orderId)
        {
            var repository   = CreateRepository();
            var order = repository.Get(orderId, EntityVersion.Latest);

            for (int i = 0; i < 15; i++)
            {
                order.AddLine($"sku{i}", 1);
            }

            repository.Save(order);
        }

        private SalesOrder CreateSalesOrder()
        {
            var order = new SalesOrder();
            order.Create(GuidProvider.NewGuid());
            CreateRepository().Save(order);
            return order;
        }

        private EventSourcingRepository<SalesOrder> CreateRepository()
        {
            var repository =
                new EventSourcingRepository<SalesOrder>(
                    _eventStore,
                    new StrictConcurrencyMonitor(),
                    new ConsoleLogger());
            return repository;
        }

        static IEnumerable<IEventStore> GetEventStore()
        {
            yield return new InMemoryEventStore(new ConsoleLogger());

            var ravenTestDriver = new RavenTestDriverAdapter();
            yield return new RavenEventStore(ravenTestDriver.CreateDocumentStore());
        }

        private class RavenTestDriverAdapter : RavenTestDriver
        {
            public RavenTestDriverAdapter()
            {
                ConfigureServer(new TestServerOptions
                {
                    ServerUrl        = "http://localhost:8080",
                    FrameworkVersion = "6.0.11",
                    CommandLineArgs  = new[] { "Security.UnsecuredAccessAllowed=PublicNetwork" }.ToList()
                });
            }

            public IDocumentStore CreateDocumentStore()
            {
                return GetDocumentStore();
            }
        }
    }
}