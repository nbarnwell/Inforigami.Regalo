using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.ObjectCompare;
using Inforigami.Regalo.Testing;
using NUnit.Framework;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class AggregateTests : TestFixtureBase
    {
        private IObjectComparer comparer;

        [SetUp]
        public void SetUp()
        {
            comparer = new ObjectComparer().Ignore<IMessageHeaders, Guid>(x => x.MessageId)
                                                 .Ignore<IEventHeaders, Guid>(x => x.CausationId)
                                                 .Ignore<IEventHeaders, Guid>(x => x.CorrelationId);

            ObjectComparisonResult.ThrowOnFail = true;
        }

        [Test]
        public void InvokingBehaviour_GivenSimpleAggregateRoot_ShouldRecordEvents()
        {
            // Arrange
            var user = new User();
            user.Register();

            // Act
            user.ChangePassword("newpassword");
            IEnumerable<IEvent> actual = user.GetUncommittedEvents();

            var expected = new EventChain
                           {
                               new UserRegistered(user.Id),
                               new UserChangedPassword("newpassword")
                           };
            
            ObjectComparisonResult result = comparer.AreEqual(expected, actual);
            if (!result.AreEqual)
            {
                throw new AssertionException(string.Format("Actual events did not match expected events. {0}", result.InequalityReason));
            }
        }

        [Test]
        public void AcceptingEvents_GivenAggregateWithUncommittedEvents_ShouldClearUncommittedEvents()
        {
            // Arrange
            var user = new User();
            user.Register();
            user.ChangePassword("newpassword");

            // Act
            IEnumerable<IEvent> expectedBefore = new EventChain { new UserRegistered(user.Id), new UserChangedPassword("newpassword") };
            IEnumerable<IEvent> expectedAfter = new IEvent[0];

            IEnumerable<IEvent> before = user.GetUncommittedEvents();
            user.AcceptUncommittedEvents();
            IEnumerable<IEvent> after = user.GetUncommittedEvents();
            
            // Assert
            var result = comparer.AreEqual(expectedBefore, before);
            if (!result.AreEqual)
            {
                throw new AssertionException(string.Format("Actual events did not match expected events. {0}", result.InequalityReason));
            }
            comparer.AreEqual(expectedAfter, after);
        }

        [Test]
        public void InvokingBehaviour_GivenAggregateWithInvariantLogic_ShouldFailIfInvariantIsNotSatisfied()
        {
            // Arrange
            var user = new User();
            user.Register();
            user.ChangePassword("newpassword");

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => user.ChangePassword("newpassword"), "Expected exception stating the new password must be different the the previous one.");
        }

        [Test]
        public void ApplyingNoEvents_GivenNewAggregateObject_ShouldNotModifyState()
        {
            // Arrange
            var user = new User();

            // Act
            user.ApplyAll(Enumerable.Empty<IEvent>());

            // Assert
            Assert.That(user.BaseVersion, Is.EqualTo(0));
        }
        
        [Test]
        public void ApplyingEventsThatHaveBaseType_GivenAnyAggregateObject_ShouldCallAppropriateApplyMethodForEachTypeInEventTypeHierarchy()
        {
            // Arrange
            var user = new User();
            var events = new IEvent[] { new UserRegistered(user.Id), new UserChangedPassword("newpassword"), new UserChangedPassword("newerpassword") };

            // Act
            user.ApplyAll(events);

            // Assert
            Assert.AreEqual(3, user.ChangeCount);
        }

        [Test]
        public void ApplyingPreviouslyGeneratedEvents_GivenNewAggregateObject_ShouldBringAggregateBackToPreviousState()
        {
            // Arrange
            var user = new User();
            user.Register();
            var events = new IEvent[] {new UserRegistered(user.Id), new UserChangedPassword("newpassword"), new UserChangedPassword("newerpassword") };

            // Act
            user.ApplyAll(events);

            // Assert
            Assert.Throws<InvalidOperationException>(() => user.ChangePassword("newerpassword"), "Expected exception stating the new password must be different the the previous one, indicating that previous events have replayed successfully.");
        }

        [Test]
        public void ApplyingPreviousEvents_GivenEventsThatWouldNotSatisfyCurrentInvariantLogic_ShouldNotFail()
        {
            // Arrange
            var userId = Guid.Parse("{42B90234-926D-4AA6-A960-F610D52F8F88}");
            var user = new User();
            var events = new IEvent[] {new UserRegistered(userId), new UserChangedPassword("newpassword"), new UserChangedPassword("newpassword") };
            
            // Act
            user.ApplyAll(events);

            // Assert
            Assert.Throws<InvalidOperationException>(() => user.ChangePassword("newpassword"), "Expected exception stating the new password must be different the the previous one, indicating that previous events have replayed successfully.");
        }

        [Test]
        public void InvokingBehaviourThatDoesntSetId_GivenNewObject_ShouldFail()
        {
            var user = new User();

            Assert.Throws<IdNotSetException>(() => user.ChangePassword("newpassword"));
        }

        [Test]
        public void InvokingBehaviourOnObjectWithNoIdThatDoesntSetTheId_ShouldFail()
        {
            var user = new User();
            var events = new IEvent[] { /*new UserRegistered(user.Id), */new UserChangedPassword("newpassword"), new UserChangedPassword("newpassword") };
            user.ApplyAll(events);

            Assert.Throws<IdNotSetException>(() => user.ChangePassword("newnewpassword"));
        }
    }
}
