using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using EventStore.ClientAPI;

namespace Inforigami.Regalo.EventStore
{
    public class EventStoreConnectionProvider : IEventStoreConnectionProvider
    {
        private readonly IEventStoreConfiguration _configuration;

        private Func<ConnectionSettingsBuilder> _buildConnectionSettings;

        private readonly Func<ConnectionSettingsBuilder> _buildConnectionSettingsDefault = () => ConnectionSettings.Create().FailOnNoServerResponse();

        public EventStoreConnectionProvider(IEventStoreConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;

            _buildConnectionSettings = _buildConnectionSettingsDefault;
        }

        public void SetConnectionSettingsBuilder(Func<ConnectionSettingsBuilder> buildConnectionSettings)
        {
            if (buildConnectionSettings == null) throw new ArgumentNullException("buildConnectionSettings");

            _buildConnectionSettings = buildConnectionSettings;
        }

        public void ResetConnectionSettingsBuilder()
        {
            _buildConnectionSettings = _buildConnectionSettingsDefault;
        }

        public IEventStoreConnection GetConnection()
        {
            switch (_configuration.ConnectionBehavior)
            {
                case EventStoreConnectionBehavior.ClusterWithDns:
                    return CreateConnectionForDnsClustering();
                case EventStoreConnectionBehavior.ClusterWithGossipSeeds:
                    return CreateConnectionForGossipSeedClustering();
                case EventStoreConnectionBehavior.NoClustering:
                    return CreateConnectionForSingleNode();
            }

            throw new InvalidOperationException(
                string.Format(
                    "Unsupported EventStore connection behavior: {0}. Valid values are: ClusterWithDns, ClusterWithGossipSeeds and NoClusteringClusterWithDns, ClusterWithGossipSeeds and NoClustering.",
                    _configuration.ConnectionBehavior));
        }

        private IEventStoreConnection CreateConnectionForSingleNode()
        {
            var endpoint = GetEndpointDetails().Select(ResolveIpEndPoint)
                                               .First();

            return EventStoreConnection.Create(GetConnectionSettings(), endpoint);
        }

        private IEventStoreConnection CreateConnectionForGossipSeedClustering()
        {
            var endpoints = GetEndpointDetails().Select(ResolveIpEndPoint)
                                                .ToArray();

            var clusterSettings = ClusterSettings.Create().DiscoverClusterViaGossipSeeds();
            clusterSettings.SetGossipSeedEndPoints(endpoints);

            return EventStoreConnection.Create(GetConnectionSettings(), clusterSettings);
        }

        private IEventStoreConnection CreateConnectionForDnsClustering()
        {
            var endpointDetails = GetEndpointDetails().First();

            var clusterSettings = ClusterSettings.Create().DiscoverClusterViaDns();
            clusterSettings.SetClusterDns(endpointDetails.Host);
            clusterSettings.SetClusterGossipPort(endpointDetails.Port);

            return EventStoreConnection.Create(GetConnectionSettings(), clusterSettings);
        }

        private IEnumerable<EventStoreEndpoint> GetEndpointDetails()
        {
            var endpointAppSetting = _configuration.EventStoreEndpoints;

            return endpointAppSetting.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => x.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                                     .Select(x => new { Host = x[0], Port = (x.Length >= 2 ? x[1] : null) })
                                     .Select(x => new EventStoreEndpoint { Host = x.Host, Port = TryParseInt(x.Port, 1113) });
        }

        private ConnectionSettingsBuilder GetConnectionSettings()
        {
            return _buildConnectionSettings();
        }

        private static IPEndPoint ResolveIpEndPoint(EventStoreEndpoint endpointDetails)
        {
            //if (endpointDetails.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return new IPEndPoint(IPAddress.Parse("127.0.0.1"), endpointDetails.Port);
            //}

            IPAddress ipAddress;
            if (!IPAddress.TryParse(endpointDetails.Host, out ipAddress))
            {
                var addresses = Dns.GetHostAddresses(endpointDetails.Host).Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToList();

                if (!addresses.Any())
                {
                    throw new InvalidOperationException(string.Format("Unable to resolve IPV4 address for hostname '{0}'.", endpointDetails.Host));
                }

                var addressBytes = addresses.First().GetAddressBytes();
                ipAddress = new IPAddress(addressBytes);
            }

            return new IPEndPoint(ipAddress, endpointDetails.Port);
        }

        private static int TryParseInt(string input, int defaultValue)
        {
            int result;
            if (int.TryParse(input, out result))
            {
                return result;
            }

            return defaultValue;
        }

        private class EventStoreEndpoint
        {
            public string Host { get; set; }
            public int Port { get; set; }
        }
    }
}