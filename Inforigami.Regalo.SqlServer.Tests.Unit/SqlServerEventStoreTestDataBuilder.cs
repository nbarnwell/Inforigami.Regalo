using Inforigami.Regalo.Core;
using Inforigami.Regalo.Testing;
using Microsoft.Extensions.Configuration;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    public class SqlServerEventStoreTestDataBuilder : ITestDataBuilder<SqlServerEventStore>
    {
        private ILogger _logger;
        private string _connectionName;
        private IConfiguration _configuration;

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