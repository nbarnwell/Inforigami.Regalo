using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;
using NUnit.Framework;

namespace Inforigami.Regalo.Testing.Tests.Unit
{
    [TestFixture]
    public class EventBusTests
    {
        [Test]
        public void GivenNoPreviousState_WhenPublishingSingleEvent_ThenEventsShouldBeStored()
        {
            // Arrange
            var eventBus = new FakeEventBus();

            // Act
            eventBus.Publish(new SomethingHappened(1));

            // Assert
            CollectionAssert.AreEqual(new object[] {new SomethingHappened(1)}, eventBus.Events);
        }

        [Test]
        public void GivenNoPreviousState_WhenPublishingEvents_ThenEventsShouldBeStored()
        {
            // Arrange
            var eventBus = new FakeEventBus();

            // Act
            eventBus.Publish((IEnumerable<IEvent>)(new IEvent[] { new SomethingHappened(1), new SomethingElseHappened(2) }));

            // Assert
            CollectionAssert.AreEqual(new object[] { new SomethingHappened(1), new SomethingElseHappened(2) }, eventBus.Events.ToArray());
        }
    }

    public class SomethingHappened : Event
    {
        public int Id { get; private set; }

        public SomethingHappened(int id)
        {
            Id = id;
        }

        protected bool Equals(SomethingHappened other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SomethingHappened)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    public class SomethingElseHappened : Event
    {   
        public int Id { get; private set; }

        public SomethingElseHappened(int id)
        {
            Id = id;
        }

        protected bool Equals(SomethingElseHappened other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SomethingElseHappened)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
