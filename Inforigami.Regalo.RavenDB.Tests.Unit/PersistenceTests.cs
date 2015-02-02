using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Moq;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.EventSourcing;
using Inforigami.Regalo.Core.Tests.Unit;
using Inforigami.Regalo.RavenDB.Tests.Unit.DomainModel.Customers;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.RavenDB.Tests.Unit
{
    [TestFixture]
    public class PersistenceTests
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
                if (type == typeof(ILogger)) return new NullLogger();
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
        public void Loading_GivenEmptyStore_ShouldReturnNull()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);

            // Act
            IEnumerable<IEvent> events = store.Load(Guid.NewGuid());

            // Assert
            CollectionAssert.IsEmpty(events);
        }

        [Test]
        public void Saving_GivenSingleEvent_ShouldAllowReloading()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);

            // Act
            var id = Guid.NewGuid();
            var evt = new CustomerSignedUp(id);
            store.Add(id, new[] { evt });
            var events = store.Load(id);

            // Assert
            Assert.NotNull(events);
            CollectionAssert.AreEqual(
                new object[] { evt },
                events,
                "Events reloaded from store do not match those generated by aggregate.");
        }

        [Test]
        public void Saving_GivenEventWithGuidProperty_ShouldAllowReloadingToGuidType()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);

            var customer = new Customer();
            customer.Signup();

            var accountManager = new AccountManager();
            var startDate = new DateTime(2012, 4, 28);
            accountManager.Employ(startDate);

            customer.AssignAccountManager(accountManager.Id, startDate);

            store.Add(customer.Id, customer.GetUncommittedEvents());

            // Act
            var acctMgrAssignedEvent = (AssignedAccountManager)store.Load(customer.Id).LastOrDefault();

            // Assert
            Assert.NotNull(acctMgrAssignedEvent);
            Assert.AreEqual(accountManager.Id, acctMgrAssignedEvent.AccountManagerId);
        }

        [Test]
        public void Saving_GivenEvents_ShouldAllowReloading()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);

            // Act
            var customer = new Customer();
            customer.Signup();
            store.Add(customer.Id, customer.GetUncommittedEvents());
            var events = store.Load(customer.Id);

            // Assert
            Assert.NotNull(events);
            CollectionAssert.AreEqual(customer.GetUncommittedEvents(), events, "Events reloaded from store do not match those generated by aggregate.");
        }


        [Test]
        public void Saving_GivenNoEvents_ShouldDoNothing()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);

            // Act
            var id = Guid.NewGuid();
            store.Add(id, Enumerable.Empty<IEvent>());
            var events = store.Load(id);

            // Assert
            CollectionAssert.IsEmpty(events);
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecificVersion_ThenShouldOnlyReturnRequestedEvents()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain().Add(new CustomerSignedUp(customerId))
                                               .Add(new SubscribedToNewsletter("latest"))
                                               .Add(new SubscribedToNewsletter("top"));
            store.Add(customerId, storedEvents);
            
            // Act
            var events = store.Load(customerId, storedEvents[1].Version);

            // Assert
            CollectionAssert.AreEqual(storedEvents.Take(2), events, "Events loaded from store do not match version requested.");
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecificVersionThatNoEventHas_ThenShouldFail()
        {
            // Arrange
            IEventStore store = new DelayedWriteRavenEventStore(_documentStore);
            var customerId = Guid.NewGuid();
            var storedEvents = new IEvent[]
                              {
                                  new CustomerSignedUp(customerId), 
                                  new SubscribedToNewsletter("latest"), 
                                  new SubscribedToNewsletter("top")
                              };
            store.Add(customerId, storedEvents);

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => store.Load(customerId, 4));
        }

        [Test]
        public void Saving_GivenEventMappedToAggregateType_ThenShouldSetRavenCollectionName()
        {
            var customerId = Guid.NewGuid();
            
            using (var eventStore = new DelayedWriteRavenEventStore(_documentStore))
            {
                Conventions.SetFindAggregateTypeForEventType(
                    type =>
                    {
                        if (type == typeof(CustomerSignedUp))
                        {
                            return typeof(Customer);
                        }

                        return typeof(EventStream);
                    });

                var storedEvents = new IEvent[]
                {
                    new CustomerSignedUp(customerId),
                    new SubscribedToNewsletter("latest"),
                    new SubscribedToNewsletter("top")
                };

                eventStore.Add(customerId, storedEvents);
                eventStore.Flush();
            }

            using (var session = _documentStore.OpenSession())
            {
                var eventStream = session.Load<EventStream>(customerId.ToString());
                var entityName = session.Advanced.GetMetadataFor(eventStream)[Constants.RavenEntityName].ToString();

                Assert.That(entityName, Is.EqualTo("Customers"));
            }
        }
    }
}
