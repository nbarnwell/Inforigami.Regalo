using Inforigami.Regalo.Interfaces;
using NUnit.Framework;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    [TestFixture]
    public class MessageCausationTests
    {
        [Test]
        public void Message_caused_by_another_receives_correct_values()
        {
            var command = new MyCommand { UserId = "3ACE2FD1-5C45-4B91-9227-3202A924F8FA" };
            var event1 = new MyEvent();
            event1.WasCausedBy(command);

            Assert.That(event1.CausationId, Is.EqualTo(command.MessageId));
            Assert.That(event1.CorrelationId, Is.EqualTo(command.MessageId));
            Assert.That(event1.CorrelationId, Is.EqualTo(command.CorrelationId));
            Assert.That(event1.UserId, Is.EqualTo(command.UserId));
            Assert.That(event1.CorrelationTimestamp, Is.EqualTo(command.CorrelationTimestamp));
        }

        public class MyCommand : Command
        {
            
        }

        public class MyEvent : Event
        {
            
        }
    }
}