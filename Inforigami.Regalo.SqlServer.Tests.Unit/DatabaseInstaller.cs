using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    public static class DatabaseInstaller
    {
        public static async Task Install()
        {
            await RunScript("UnitTestSetUpConnection", ".\\CreateDatabase.sql");
            await RunScript("SqlClientConnection", ".\\InstallEventStreamTables.sql");
        }

        private static async Task RunScript(string connectionName, string filename)
        {
            using var connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionName].ConnectionString);
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