using System;
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

        [Test]
        public void Message_causing_another_receives_correct_values()
        {
            var command = new MyCommand { UserId = "3ACE2FD1-5C45-4B91-9227-3202A924F8FA" };
            var event1 = command.Causes(cmd => new MyEvent());
            event1.WasCausedBy(command);

            Assert.That(event1.CausationId, Is.EqualTo(command.MessageId));
            Assert.That(event1.CorrelationId, Is.EqualTo(command.MessageId));
            Assert.That(event1.CorrelationId, Is.EqualTo(command.CorrelationId));
            Assert.That(event1.UserId, Is.EqualTo(command.UserId));
            Assert.That(event1.CorrelationTimestamp, Is.EqualTo(command.CorrelationTimestamp));
        }

        [Test]
        public void Message_with_MinValue_timestamp_can_be_updated_from_another_with_current_date()
        {
            var causer = new MyCommand();
            causer.Timestamp = new DateTimeOffset(2017, 10, 9, 14, 38, 0, TimeSpan.FromHours(1));

            var causee = new MyEvent();
            causee.Timestamp = new DateTimeOffset();

            causee.WasCausedBy(causer);
        }

        public class MyCommand : Command
        {
            
        }

        public class MyEvent : Event
        {
            
        }
    }
}