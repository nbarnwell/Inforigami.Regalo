using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    public static class DatabaseInstaller
    {
        private static IConfiguration Configuration;

        static DatabaseInstaller()
        {
            Configuration =
                new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                          .Build();
        }

        public static async Task Install()
        {
            await RunScript("UnitTestSetUpConnection", ".\\CreateDatabase.sql");
            await RunScript("RegaloConnection", ".\\InstallEventStreamTables.sql");
        }

        private static async Task RunScript(string connectionName, string filename)
        {
            var connectionString = Configuration.GetConnectionString(connectionName);
            using (var connection = new SqlConnection(connectionString))
            {
                var filepath = Path.GetFullPath(filename);

                var sql = File.ReadAllText(filepath);

                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}