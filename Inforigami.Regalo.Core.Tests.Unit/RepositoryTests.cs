using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Testing;
using Moq;
using NUnit.Framework;
using Inforigami.Regalo.Core.EventSourcing;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class RepositoryTests : TestFixtureBase
    {
        private IObjectComparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = new ObjectComparer().Ignore<IMessageHeaders, Guid>(x => x.MessageId)
                                            .Ignore<IEventHeaders, Guid>(x => x.CausationId)
                                            .Ignore<IEventHeaders, Guid>(x => x.CorrelationId);

            ObjectComparisonResult.ThrowOnFail = true;
        }

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
            CollectionAssertAreJsonEqual(expectedEvents, eventStore.GetAllEvents());
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
            var comparisonResult = _comparer.AreEqual(expectedEvents, eventStore.GetAllEvents());
            Assert.That(comparisonResult.AreEqual, comparisonResult.InequalityReason);
        }

        [Test]
        public void GivenPopulatedEventStore_WhenLoadingAggregate_ThenRepositoryShouldRebuildThatAggregate()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var userId = Guid.NewGuid();
            eventStore.Save<User>(
                EventStreamIdFormatter.GetStreamId<User>(userId.ToString()),
                0,
                new EventChain()
                {
                    new UserRegistered(userId),
                    new UserChangedPassword("newpassword"),
                    new UserChangedPassword("newnewpassword")
                });
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId, 3);

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
            eventStore.Save<User>(EventStreamIdFormatter.GetStreamId<User>(userId.ToString()), 0, events);

            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId, events[1].Headers.Version);

            // Assert
            Assert.AreEqual(events[1].Headers.Version, user.BaseVersion);
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
            eventStore.Save<User>(
                EventStreamIdFormatter.GetStreamId<User>(userId.ToString()),
                0,
                new EventChain
                {
                    new UserRegistered(userId),
                    new UserChangedPassword("newpassword"),
                    new UserChangedPassword("newnewpassword")
                });
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);

            // Act
            User user = repository.Get(userId, 3);

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
            var eventStore = new InMemoryEventStore();

            // Register and save a new user
            var user = new User();
            user.Register();
            var concurrencyMonitor = new StrictConcurrencyMonitor();
            var repository = new EventSourcingRepository<User>(eventStore, concurrencyMonitor);
            repository.Save(user);

            // We change password...
            var repository1 = new EventSourcingRepository<User>(eventStore, concurrencyMonitor);
            var user1 = repository1.Get(user.Id, user.Version);
            user1.ChangePassword("newpassword");

            // ...but so does someone else, who saves their changes before we have a chance to save ours...
            var repository2 = new EventSourcingRepository<User>(eventStore, concurrencyMonitor);
            var user2 = repository.Get(user.Id, user.Version);
            user2.ChangePassword("newpassword");
            repository2.Save(user2);

            // ...so we should get a concurrency exception when we try to save
            Assert.Throws<ConcurrencyConflictsDetectedException>(() => repository1.Save(user1));
        }

        [Test]
        public void GivenAggregateWithUncommittedEvents_WhenSaved_ThenBaseVersionShouldMatchCurrentVersion()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var user = new User();
            user.Register();
            var repository = new EventSourcingRepository<User>(eventStore, new Mock<IConcurrencyMonitor>().Object);
            repository.Save(user);

            user.ChangePassword("newpassword");

            var versionAfterChange = user.GetUncommittedEvents().Last().Headers.Version;

            // Act
            repository.Save(user);

            // Assert
            Assert.AreEqual(versionAfterChange, user.BaseVersion, "User's base version has not been updated to match current version on successful save.");
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
