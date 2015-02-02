using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Moq;
using NUnit.Framework;
using Inforigami.Regalo.Core.EventSourcing;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class RepositoryTests : TestFixtureBase
    {
        [Test]
        public void GivenAggregateWithNoUncommittedEvents_WhenSaved_ThenEventStoreShouldContainNoAdditionalEvents()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);
            var user = new User();

            var expectedEvents = Enumerable.Empty<object>();

            // Act
            repository.Save(user);

            // Assert
            CollectionAssertAreJsonEqual(expectedEvents, eventStore.Events);
        }

        [Test]
        public void GivenAggregateWithUncommittedEvents_WhenSaved_ThenEventStoreShouldContainThoseEvents()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            IRepository<User> repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);
            var user = new User();
            user.Register();

            var expectedEvents = new object[] { new UserRegistered(user.Id) };

            // Act
            repository.Save(user);

            // Assert
            CollectionAssertAreJsonEqual(expectedEvents, eventStore.Events);
        }

        [Test]
        public void GivenPopulatedEventStore_WhenLoadingAggregate_ThenRepositoryShouldRebuildThatAggregate()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var userId = Guid.NewGuid();
            eventStore.Update(
                userId,
                new EventChain()
                {
                    new UserRegistered(userId),
                    new UserChangedPassword("newpassword"),
                    new UserChangedPassword("newnewpassword")
                });
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId);

            // Assert
            Assert.Throws<InvalidOperationException>(() => user.ChangePassword("newnewpassword")); // Should fail if the user's events were correctly applied
        }

        [Test]
        public void GivenPopulatedEventStore_WhenLoadingSpecificVersionOfAggregate_ThenRepositoryShouldRebuildThatAggregateToThatVersion()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var userId = Guid.NewGuid();
            var events = new EventChain
            {
                new UserRegistered(userId), 
                new UserChangedPassword("newpassword"), 
                new UserChangedPassword("newnewpassword")
            };
            eventStore.Update(userId, events);
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId, ((IEvent)events[1]).Version);

            // Assert
            Assert.AreEqual(((IEvent)events[1]).Version, user.BaseVersion);
        }

        [Test]
        public void GivenAnyAggregateRoot_WhenBehaviourIsInvokedAndEventsRaised_ThenBaseVersionShouldNotChange()
        {
            // Arrange
            var user = new User();
            user.Register();
            var baseVersion = user.BaseVersion;

            // Act
            user.ChangePassword("newpassword");

            // Assert
            Assert.AreEqual(baseVersion, user.BaseVersion, "User's base version is not correct.");
        }

        [Test]
        public void GivenPopulatedEventStore_WhenLoadingAggregate_ThenAggregateVersionShouldReflectStoredEvents()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var userId = Guid.NewGuid();
            eventStore.Update(
                userId,
                new IEvent[]
                {
                    new UserRegistered(userId) { Version = 1 },
                    new UserChangedPassword("newpassword"){Version = 2},
                    new UserChangedPassword("newnewpassword") { Version = 3 }
                });
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId);

            // Assert
            Assert.AreEqual(3, user.BaseVersion);
        }

        [Test]
        public void GivenNewAggreateWithNoEvents_WhenSaving_ThenShouldNotBotherCheckingConcurrency()
        {
            // Arrange
            var user = new User();
            var concurrencyMonitor = new Mock<IConcurrencyMonitor>();
            var repository = new EventSourcingRepository<User>(new InMemoryEventStore(), concurrencyMonitor.Object);

            // Act
            repository.Save(user);

            // Assert
            concurrencyMonitor.Verify(monitor => monitor.CheckForConflicts(It.IsAny<IEnumerable<IEvent>>(), It.IsAny<IEnumerable<IEvent>>()), Times.Never());
        }

        [Test]
        public void GivenNewAggreateWithNewEvents_WhenSaving_ThenShouldNotBotherCheckingConcurrency()
        {
            // Arrange
            var user = new User();
            user.Register();
            var concurrencyMonitor = new Mock<IConcurrencyMonitor>();
            var repository = new EventSourcingRepository<User>(new InMemoryEventStore(), concurrencyMonitor.Object);

            // Act
            repository.Save(user);

            // Assert
            concurrencyMonitor.Verify(monitor => monitor.CheckForConflicts(It.IsAny<IEnumerable<IEvent>>(), It.IsAny<IEnumerable<IEvent>>()), Times.Never());
        }

        [Test]
        public void GivenExistingAggregateWithUnseenChanges_WhenSaving_ThenShouldCheckConcurrencyWithCorrectEvents()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventStore = new InMemoryEventStore();
            eventStore.Update(userId, new UserRegistered(userId));

            var concurrencyMonitor = new StrictConcurrencyMonitor();
            var repository = new EventSourcingRepository<User>(eventStore, concurrencyMonitor);
            var user = repository.Get(userId);

            // Now another user changes the password before we get chance to save our changes:
            eventStore.Update(userId, new UserChangedPassword("adifferentpassword"));

            user.ChangePassword("newpassword");

            Assert.Throws<ConcurrencyConflictsDetectedException>(() => repository.Save(user));
        }

        [Test]
        public void GivenAggregateWithUncommittedEvents_WhenSaved_ThenBaseVersionShouldMatchCurrentVersion()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventStore = new InMemoryEventStore();
            var userRegistered = new UserRegistered(userId);
            eventStore.Update(userId, userRegistered);

            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);
            var user = repository.Get(userId);
            user.ChangePassword("newpassword");

            var currentVersion = user.GetUncommittedEvents().Cast<IEvent>().Last().Version;

            // Act
            repository.Save(user);

            // Assert
            Assert.AreEqual(currentVersion, user.BaseVersion, "User's base version has not been updated to match current version on successful save.");
        }

        [Test]
        public void GivenAggregateWithUncommittedEvents_WhenSaving_ThenUncommittedEventsShouldBeAccepted()
        {
            // Arrange
            var user = new User();
            user.Register();
            var repository = new EventSourcingRepository<User>(new InMemoryEventStore(), new Mock<IConcurrencyMonitor>().Object);

            // Act
            repository.Save(user);

            // Assert
            CollectionAssert.IsEmpty(user.GetUncommittedEvents());
        }

        //Need new test for variations of conventions
    }
}
