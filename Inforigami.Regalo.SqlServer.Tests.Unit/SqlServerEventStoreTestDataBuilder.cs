using Inforigami.Regalo.Core;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    public class SqlServerEventStoreTestDataBuilder : ITestDataBuilder<SqlServerEventStore>
    {
        private ILogger _logger;
        private string _connectionName;

        public string CurrentDescription { get; }

        public SqlServerEventStoreTestDataBuilder()
        {
            _logger = new ConsoleLogger();
            _connectionName = "SqlClientConnection";
        }

        public SqlServerEventStore Build()
        {
            return new SqlServerEventStore(_connectionName, _logger);
        }
    }
}