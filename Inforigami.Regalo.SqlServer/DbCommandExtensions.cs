using System.Data;
using System.Data.Common;

namespace Inforigami.Regalo.SqlServer
{
    internal static class DbCommandExtensions
    {
        public static DbParameter AddParameter(this DbCommand command, string name, DbType dbType, int size)
        {
            var parameter = command.AddParameter(name, dbType);
            parameter.Size = size;
            return parameter;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, DbType dbType)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = dbType;
            command.Parameters.Add(parameter);
            return parameter;
        }

        public static DbParameter AddParameterWithValue(this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
            return parameter;
        }
    }
}
