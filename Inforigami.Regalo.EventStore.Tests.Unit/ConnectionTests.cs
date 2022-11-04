using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Inforigami.Regalo.EventStore.Tests.Unit
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        public async Task Connect_to_single_node()
        {
            var connection = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"));
            await connection.ConnectAsync();

            const string streamName = "newstream";
            const string eventType  = "event-type";
            const string data       = "{ \"a\":\"2\"}";
            const string metadata   = "{}";

            var eventPayload = new EventData(
                eventId: Guid.NewGuid(),
                type: eventType,
                isJson: true,
                data: Encoding.UTF8.GetBytes(data),
                metadata: Encoding.UTF8.GetBytes(metadata)
            );
            var result     = await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventPayload);
            var readEvents = await connection.ReadStreamEventsForwardAsync(streamName, 0, 10, true);

            foreach (var evt in readEvents.Events)
            {
                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));
            }
        }
    }
}