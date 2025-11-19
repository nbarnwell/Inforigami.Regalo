using System.Data.Common;

namespace Inforigami.Regalo.SqlServer
{
    internal static class SqlSessionExtensions
    {
        public static DbCommand CreateCommand(this ISqlSession sqlSession)
        {
            var command = sqlSession.Connection.CreateCommand();
            command.Transaction = sqlSession.Transaction;
            return command;
        }
    }
}
