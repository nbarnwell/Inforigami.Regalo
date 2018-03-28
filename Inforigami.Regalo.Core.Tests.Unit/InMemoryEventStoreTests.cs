using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Testing;
using NUnit.Framework;
using Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class InMemoryEventStoreTests : TestFixtureBase
    {
        [Test]
        public void GivenEmptyEventStore_WhenLoadingEvents_ThenNothingShouldBeReturned()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());
            
            // Act
            EventStream<SalesOrder> stream = store.Load<SalesOrder>(Guid.NewGuid().ToString());

            // Assert
            Assert.That(stream, Is.Null);
        }

        [Test]
        public void GivenEmptyEventStore_WhenLoadingEventsForSpecificVersion_ThenNothingShouldBeReturned()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());

            // Act
            EventStream<SalesOrder> stream = store.Load<SalesOrder>(Guid.NewGuid().ToString(), 1);

            // Assert
            Assert.That(stream, Is.Null);
        }

        [Test]
        public void GivenEmptyEventStore_WhenAddingEventsOneAtATime_ThenStoreShouldContainThoseEvents()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());
            var userId = Guid.NewGuid();
            var userRegistered = new UserRegistered(userId);

            // Act
            store.Save<User>(userId.ToString(), 0, new[] { userRegistered });

            // Assert
            var allEvents = ((InMemoryEventStore)store).GetAllEvents();
            CollectionAssert.IsNotEmpty(allEvents);
            Assert.AreSame(userRegistered, allEvents.First());
        }

        [Test]
        public void GivenEmptyEventStore_WhenAddingEventsInBulk_ThenStoreShouldContainThoseEvents()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());
            var user = new User();
            user.Register();
            user.ChangePassword("newpassword");

            // Act
            store.Save<User>(user.Id.ToString(), 0, user.GetUncommittedEvents());

            // Assert
            var allEvents = ((InMemoryEventStore)store).GetAllEvents();
            CollectionAssert.IsNotEmpty(allEvents);
            CollectionAssert.AreEqual(user.GetUncommittedEvents(), allEvents);
        }

        [Test]
        public void GivenEventStorePopulatedWithEventsForMultipleAggregates_WhenLoadingEventsForAnAggregate_ThenShouldReturnEventsForThatAggregate()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());

            var user1 = new User();
            user1.Register();
            user1.ChangePassword("user1pwd1");
            user1.ChangePassword("user1pwd2");

            var user2 = new User();
            user2.Register();
            user2.ChangePassword("user2pwd1");
            user2.ChangePassword("user2pwd2");

            store.Save<User>(user1.Id.ToString(), 0, user1.GetUncommittedEvents());
            store.Save<User>(user2.Id.ToString(), 0, user2.GetUncommittedEvents());

            // Act
            EventStream<User> eventsForUser1 = store.Load<User>(user1.Id.ToString());

            // Assert
            CollectionAssert.AreEqual(user1.GetUncommittedEvents(), eventsForUser1.Events, "Store didn't return user1's events properly.");
        }

        [Test]
        public void GivenEventStorePopulatedWithEventsForMultipleAggregates_WhenLoadingEventsForAnAggregate_AndSpecifyingLatestVersion_ThenShouldReturnEventsForThatAggregate()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());

            var user1 = new User();
            user1.Register();
            user1.ChangePassword("user1pwd1");
            user1.ChangePassword("user1pwd2");

            var user2 = new User();
            user2.Register();
            user2.ChangePassword("user2pwd1");
            user2.ChangePassword("user2pwd2");

            store.Save<User>(user1.Id.ToString(), 0, user1.GetUncommittedEvents());
            store.Save<User>(user2.Id.ToString(), 0, user2.GetUncommittedEvents());

            // Act
            EventStream<User> eventsForUser1 = store.Load<User>(user1.Id.ToString(), EntityVersion.Latest);

            // Assert
            CollectionAssert.AreEqual(user1.GetUncommittedEvents(), eventsForUser1.Events, "Store didn't return user1's events properly.");
        }

        [Test]
        public void GivenEventStorePopulatedWithManyEventsForAnAggregate_WhenLoadingForSpecificVersion_ThenShouldOnlyLoadEventsUpToAndIncludingThatVersion()
        {
            // Arrange
            IEventStore store = new InMemoryEventStore(new ConsoleLogger());
            var id = Guid.NewGuid();
            var allEvents = new EventChain().Add(new UserRegistered(id))
                                            .Add(new UserChangedPassword("pwd1"))
                                            .Add(new UserChangedPassword("pwd2"))
                                            .Add(new UserChangedPassword("pwd3"))
                                            .Add(new UserChangedPassword("pwd4"));

            store.Save<User>(id.ToString(), 0, allEvents);

            // Act
            IEnumerable<IEvent> version3 = store.Load<User>(id.ToString(), allEvents[2].Version).Events;

            // Assert
            CollectionAssert.AreEqual(allEvents.Take(3).ToArray(), version3);
        }
    }
}
