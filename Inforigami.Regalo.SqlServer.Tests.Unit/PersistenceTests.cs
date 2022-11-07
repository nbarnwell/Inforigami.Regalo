﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.SqlServer.Tests.Unit.DomainModel.Customers;
using Inforigami.Regalo.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ILogger = Inforigami.Regalo.Core.ILogger;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    [TestFixture]
    public class PersistenceTests
    {
        [SetUp]
        public async Task SetUp()
        {
            Resolver.Configure(type =>
            {
                if (type == typeof(ILogger)) return new ConsoleLogger();
                throw new InvalidOperationException(string.Format("No type of {0} registered.", type));
            },
            type => null,
            o => { });

            await DatabaseInstaller.Install();
        }

        [TearDown]
        public void TearDown()
        {
            Conventions.SetFindAggregateTypeForEventType(null);

            Resolver.Reset();
        }

        [Test]
        public void Loading_GivenEmptyStore_ShouldReturnNull()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();

            // Act
            EventStream<Customer> events = store.Load<Customer>(Guid.NewGuid().ToString());

            // Assert
            Assert.That(events, Is.Null);
        }

        [Test]
        public void Saving_GivenSingleEvent_ShouldAllowReloading()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();

            // Act
            var id = Guid.NewGuid();
            var evt = new CustomerSignedUp(id);
            store.Save<Customer>(id.ToString(), EntityVersion.New, new[] { evt });
            var stream = store.Load<Customer>(id.ToString());

            // Assert
            Assert.NotNull(stream);
            CollectionAssert.AreEqual(
                new object[] { evt },
                stream.Events,
                "Events reloaded from store do not match those generated by aggregate.");
        }

        [Test]
        public void Saving_GivenEventWithGuidProperty_ShouldAllowReloadingToGuidType()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();

            var customer = new Customer();
            customer.Signup();

            var accountManager = new AccountManager();
            var startDate = new DateTime(2012, 4, 28);
            accountManager.Employ(startDate);

            customer.AssignAccountManager(accountManager.Id, startDate);

            store.Save<Customer>(customer.Id.ToString(), EntityVersion.New, customer.GetUncommittedEvents());

            // Act
            var acctMgrAssignedEvent = (AccountManagerAssigned)store.Load<Customer>(customer.Id.ToString())
                                                                    .Events
                                                                    .LastOrDefault();

            // Assert
            Assert.NotNull(acctMgrAssignedEvent);
            Assert.AreEqual(accountManager.Id, acctMgrAssignedEvent.AccountManagerId);
        }

        [Test]
        public void Saving_GivenEvents_ShouldAllowReloading()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();

            // Act
            var customer = new Customer();
            customer.Signup();
            store.Save<Customer>(customer.Id.ToString(), EntityVersion.New, customer.GetUncommittedEvents());
            var stream = store.Load<Customer>(customer.Id.ToString());

            // Assert
            Assert.NotNull(stream);
            CollectionAssert.AreEqual(customer.GetUncommittedEvents(), stream.Events, "Events reloaded from store do not match those generated by aggregate.");
        }


        [Test]
        public void Saving_GivenNoEvents_ShouldDoNothing()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();

            // Act
            var id = Guid.NewGuid();
            store.Save<Customer>(id.ToString(), EntityVersion.New, Enumerable.Empty<IEvent>());
            var stream = store.Load<Customer>(id.ToString());

            // Assert
            Assert.That(stream, Is.Null);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecificVersion_ThenShouldOnlyReturnRequestedEvents(int version)
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain().Add(new CustomerSignedUp(customerId))
                                               .Add(new SubscribedToNewsletter("latest"))
                                               .Add(new SubscribedToNewsletter("top"));
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);

            // Act
            var stream = store.Load<Customer>(customerId.ToString(), version);

            // Assert
            CollectionAssert.AreEqual(storedEvents.Take(version + 1), stream.Events, "Events loaded from store do not match version requested.");
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingMaxVersion_ThenShouldReturnAllEvents()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain().Add(new CustomerSignedUp(customerId))
                                               .Add(new SubscribedToNewsletter("latest"))
                                               .Add(new SubscribedToNewsletter("top"));
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);

            // Act
            var stream = store.Load<Customer>(customerId.ToString(), EntityVersion.Latest);

            // Assert
            CollectionAssert.AreEqual(storedEvents, stream.Events, "Events loaded from store do not match version requested.");
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecificVersionThatNoEventHas_ThenShouldFail()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();
            var customerId = Guid.NewGuid();
            var storedEvents = new IEvent[]
                              {
                                  new CustomerSignedUp(customerId), 
                                  new SubscribedToNewsletter("latest"), 
                                  new SubscribedToNewsletter("top")
                              };
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => store.Load<Customer>(customerId.ToString(), 10));
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecialNoStreamVersion_ThenShouldFail()
        {
            // Arrange
            IEventStore store = new SqlServerEventStoreTestDataBuilder().Build();
            var customerId = Guid.NewGuid();
            var storedEvents = new IEvent[]
                              {
                                  new CustomerSignedUp(customerId), 
                                  new SubscribedToNewsletter("latest"), 
                                  new SubscribedToNewsletter("top")
                              };
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => store.Load<Customer>(customerId.ToString(), EntityVersion.New));
        }
    }
}