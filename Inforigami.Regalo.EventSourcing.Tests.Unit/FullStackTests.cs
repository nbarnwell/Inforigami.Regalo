using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.AzureTableStorage;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders;
using Inforigami.Regalo.EventSourcing.Tests.Unit.Support;
using Inforigami.Regalo.RavenDB;
using Inforigami.Regalo.SqlServer;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

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
            AddLineItemsInBatch(order.Id);
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

        private void AddLineItemsInBatch(Guid orderId)
        {
            var repository = CreateRepository();
            var order      = repository.Get(orderId, EntityVersion.Latest);

            for (int i = 0; i < 15; i++)
            {
                order.AddLine($"sku{i}", 1);
            }

            repository.Save(order);
        }

        private void AddLineItems(Guid orderId)
        {
            // This is to see if we fall foul of Raven's max requests per session limit
            var repository = CreateRepository();
            for (int i = 15; i < 60; i++)
            {
                var order = repository.Get(orderId, EntityVersion.Latest);
                order.AddLine($"sku{i}", 1);
                repository.Save(order);
            }
            _eventStore.Flush();
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

            yield return new RavenEventStore(new RavenTestDriverAdapter().CreateDocumentStore());

            yield return NewAzureTableStorageEventStore();

            DatabaseInstaller.Install().ConfigureAwait(false).GetAwaiter().GetResult();
            yield return new SqlServerEventStoreTestDataBuilder().Build();
        }

        private static AzureTableStorageEventStore NewAzureTableStorageEventStore()
        {
            var name = GetEnvironmentVariable("Inforigami_Regalo_AzureTableStorage_UnitTest_AzureStorageAccountName");
            var key  = GetEnvironmentVariable("Inforigami_Regalo_AzureTableStorage_UnitTest_AzureStorageAccountKey");
            return new AzureTableStorageEventStore(name, key, new ConsoleLogger());
        }

        private static string GetEnvironmentVariable(string envVarName)
        {
            var result = Environment.GetEnvironmentVariable(envVarName);
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException(
                    $@"Unable to find environment variable value for {envVarName}");
            }
            return result;
        }

        private class RavenTestDriverAdapter
        {
            private readonly IDocumentStore _documentStore;

            public RavenTestDriverAdapter()
            {
                _documentStore = new DocumentStore()
                {
                    Urls     = new[] { "http://localhost:8080" },
                    Database = "Inforigami.Regalo.EventSourcing.Tests.Unit"
                };
                _documentStore.Initialize();

                var record = _documentStore.Maintenance.Server.Send(new GetDatabaseRecordOperation(_documentStore.Database));
                if (record == null)
                {
                    _documentStore.Maintenance.Server.Send(
                        new CreateDatabaseOperation(new DatabaseRecord(_documentStore.Database)));
                }
            }

            public IDocumentStore CreateDocumentStore()
            {
                return _documentStore;
            }
        }
    }
}