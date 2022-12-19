using Inforigami.Regalo.Core;
using Inforigami.Regalo.SqlServer;
using Inforigami.Regalo.Testing;
using Microsoft.Extensions.Configuration;

namespace Inforigami.Regalo.EventSourcing.Tests.Unit.Support
{
    public class SqlServerEventStoreTestDataBuilder : ITestDataBuilder<SqlServerEventStore>
    {
        private readonly ILogger _logger;
        private readonly string _connectionName;
        private readonly IConfiguration _configuration;

        public string CurrentDescription { get; }

        public SqlServerEventStoreTestDataBuilder()
        {
            _logger = new ConsoleLogger();
            _connectionName = "RegaloConnection";
            _configuration =
                new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                          .Build();
        }

        public SqlServerEventStore Build()
        {
            return new SqlServerEventStore(_configuration.GetConnectionString(_connectionName), _logger);
        }
    }
}