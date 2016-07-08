using System;
using Inforigami.Regalo.Interfaces;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.RavenDB.Tests.Unit.DomainModel.Customers;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.RavenDB.Tests.Unit
{
    [TestFixture]
    public class DelayedWriteEventStoreTests
    {
        private IDocumentStore _documentStore;
        
        [SetUp]
        public void SetUp()
        {
            _documentStore = new EmbeddableDocumentStore { RunInMemory = true };
            //_documentStore = new DocumentStore
            //{
            //    Url = "http://localhost:8080/",
            //    DefaultDatabase = "Inforigami.Regalo.RavenDB.Tests.UnitPersistenceTests"
            //};
            _documentStore.Initialize();
            Resolver.Configure(type =>
                               {
                                   if (type == typeof(ILogger)) return new ConsoleLogger();
                                   throw new InvalidOperationException(string.Format("No type of {0} registered.", type));
                               },
                type => null,
                o => { });
        }

        [TearDown]
        public void TearDown()
        {
            Conventions.SetFindAggregateTypeForEventType(null);

            Resolver.Reset();

            _documentStore.Dispose();
            _documentStore = null;
        }

        [Test]
        public void Disposing_a_delayedwriteeventstore_with_pending_changes_should_throw_exception()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    using (var eventStore = new DelayedWriteRavenEventStore(_documentStore))
                    {
                        var customerId = Guid.NewGuid();

                        var storedEvents = new EventChain
                                           {
                                               new CustomerSignedUp(customerId),
                                               new SubscribedToNewsletter("latest"),
                                               new SubscribedToNewsletter("top")
                                           };

                        eventStore.Save<Customer>(customerId.ToString(), 0, storedEvents);
                    }
                });
        }
    }
}
