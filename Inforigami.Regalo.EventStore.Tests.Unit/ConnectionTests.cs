using System;
using System.Linq;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using NUnit.Framework;

namespace Inforigami.Regalo.EventStore.Tests.Unit
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        public void Connect_to_single_node()
        {
            var config = new ConnectionConfiguration
                         {
                             ConnectionBehavior = EventStoreConnectionBehavior.NoClustering,
                             EventStoreEndpoints = "localhost:1113"
                         };
            TestConnection(config);
        }

        private static void TestConnection(IEventStoreConfiguration config)
        {
            var connectionProvider = new EventStoreConnectionProvider(config);
            connectionProvider.SetConnectionSettingsBuilder(
                () => ConnectionSettings.Create()
                                        .FailOnNoServerResponse()
                                        .SetDefaultUserCredentials(new UserCredentials("Inforigami.Regalo.EventStore", "changeit")));

            var connection = connectionProvider.GetConnection();
            Assert.DoesNotThrow(() => connection.ConnectAsync().Wait());
            Assert.DoesNotThrow(
                () =>
                {
                    try
                    {
                        connection.ReadAllEventsForwardAsync(Position.Start, 512, false).Wait();
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerExceptions.Any(x => x is AccessDeniedException))
                        {
                            throw new InvalidOperationException(
                                "Unable to authenticate with the eventstore instance at 'localhost:1113'. Ensure there is an admin user "
                                + "configured called 'Inforigami.Regalo.EventStore', with password 'changeit'.", e);
                        }
                    }
                });
        }

        private class ConnectionConfiguration : IEventStoreConfiguration
        {
            public EventStoreConnectionBehavior ConnectionBehavior { get; set; }

            public string EventStoreEndpoints { get; set; }
        }
    }
}