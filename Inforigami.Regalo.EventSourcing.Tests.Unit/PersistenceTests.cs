﻿using System;
using System.Linq;
using System.Net;
using Inforigami.Regalo.AzureTableStorage;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.EventSourcing.Tests.Unit.DomainModel.Customers;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Testing;
using NUnit.Framework;
using ILogger = Inforigami.Regalo.Core.ILogger;

namespace Inforigami.Regalo.EventSourcing.Tests.Unit
{
    [TestFixture]
    public class PersistenceTests
    {
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new ConsoleLogger();

            Resolver.Configure(
                type =>
                {
                    if (type == typeof(ILogger))
                    {
                        return _logger;
                    }

                    throw new InvalidOperationException($"No type of {type} registered.");
                },
                type => null,
                o => { });
        }

        [TearDown]
        public void TearDown()
        {
            Conventions.SetFindAggregateTypeForEventType(null);

            Resolver.Reset();

            _logger = null;
        }

        [Test]
        public void Loading_GivenEmptyStore_ShouldReturnNull()
        {
            // Arrange
            IEventStore store = NewEventStore();

            // Act
            EventStream<Customer> events = store.Load<Customer>(Guid.NewGuid().ToString());

            // Assert
            Assert.That(events, Is.Null);
        }

        [Test]
        public void Saving_GivenSingleEvent_ShouldAllowReloading()
        {
            // Arrange
            var store = NewEventStore();

            // Act
            var id = Guid.NewGuid();
            var evt = new CustomerSignedUp(id);
            store.Save<Customer>(id.ToString(), EntityVersion.New, new[] { evt });
            store.Flush();
            var stream = store.Load<Customer>(id.ToString());

            // Assert
            Assert.NotNull(stream);
            CollectionAssert.AreEqual(
                new object[] { evt },
                stream.Events,
                "Events reloaded from store do not match those generated by aggregate.");
        }

        [Test]
        public void Saving_GivenSingleEventNotFlushed_ShouldAllowReloading()
        {
            // Arrange
            var store = NewEventStore();

            // Act
            var id = Guid.NewGuid();
            var evt = new CustomerSignedUp(id);
            store.Save<Customer>(id.ToString(), EntityVersion.New, new[] { evt });
            //store.Flush();
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
            var store = NewEventStore();

            var customer = new Customer();
            customer.Signup();

            var accountManager = new AccountManager();
            var startDate = new DateTime(2012, 4, 28);
            accountManager.Employ(startDate);

            customer.AssignAccountManager(accountManager.Id, startDate);

            store.Save<Customer>(customer.Id.ToString(), EntityVersion.New, customer.GetUncommittedEvents());
            store.Flush();

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
            var store = NewEventStore();

            // Act
            var customer = new Customer();
            customer.Signup();
            store.Save<Customer>(customer.Id.ToString(), EntityVersion.New, customer.GetUncommittedEvents());
            store.Flush();
            var stream = store.Load<Customer>(customer.Id.ToString());

            // Assert
            Assert.NotNull(stream);
            CollectionAssert.AreEqual(customer.GetUncommittedEvents(), stream.Events, "Events reloaded from store do not match those generated by aggregate.");
        }

        [Test]
        public void Saving_GivenNoEvents_ShouldDoNothing()
        {
            // Arrange
            var store = NewEventStore();

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
            var store = NewEventStore();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain().Add(new CustomerSignedUp(customerId))
                                               .Add(new SubscribedToNewsletter("latest"))
                                               .Add(new SubscribedToNewsletter("top"));
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);
            store.Flush();

            // Act
            var stream = store.Load<Customer>(customerId.ToString(), version);

            // Assert
            CollectionAssert.AreEqual(storedEvents.Take(version + 1), stream.Events, "Events loaded from store do not match version requested.");
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingMaxVersion_ThenShouldReturnAllEvents()
        {
            // Arrange
            var store = NewEventStore();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain().Add(new CustomerSignedUp(customerId))
                                               .Add(new SubscribedToNewsletter("latest"))
                                               .Add(new SubscribedToNewsletter("top"));
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);
            store.Flush();

            // Act
            var stream = store.Load<Customer>(customerId.ToString(), EntityVersion.Latest);

            // Assert
            CollectionAssert.AreEqual(storedEvents, stream.Events, "Events loaded from store do not match version requested.");
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecificVersionThatNoEventHas_ThenShouldFail()
        {
            // Arrange
            var store = NewEventStore();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain
                              {
                                  new CustomerSignedUp(customerId), 
                                  new SubscribedToNewsletter("latest"), 
                                  new SubscribedToNewsletter("top")
                              };
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);
            store.Flush();

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => store.Load<Customer>(customerId.ToString(), 10));
        }

        [Test]
        public void GivenAggregateWithMultipleEvents_WhenLoadingSpecialNoStreamVersion_ThenShouldFail()
        {
            // Arrange
            var store = NewEventStore();
            var customerId = Guid.NewGuid();
            var storedEvents = new EventChain
                              {
                                  new CustomerSignedUp(customerId), 
                                  new SubscribedToNewsletter("latest"), 
                                  new SubscribedToNewsletter("top")
                              };
            store.Save<Customer>(customerId.ToString(), EntityVersion.New, storedEvents);

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => store.Load<Customer>(customerId.ToString(), EntityVersion.New));
        }

        private AzureTableStorageEventStore NewEventStore()
        {
            var name = GetEnvironmentVariable("Inforigami_Regalo_AzureTableStorage_UnitTest_AzureStorageAccountName");
            var key  = GetEnvironmentVariable("Inforigami_Regalo_AzureTableStorage_UnitTest_AzureStorageAccountKey");
            return new AzureTableStorageEventStore(name, key, _logger);
        }

        private static string GetEnvironmentVariable(string envVarName)
        {
            var result = Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException(
                    $@"Unable to find USER environment variable value for {envVarName}");
            }
            return result;
        }
    }
}