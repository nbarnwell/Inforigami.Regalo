using System.Data.Common;

namespace Inforigami.Regalo.SqlServer
{
    /// <summary>
    /// Represents a <see cref="DbConnection" /> and <see cref="DbTransaction" /> that are externally controlled.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of the external owner to manage and dispose of the connection and transaction.
    /// The <see cref="Complete" /> and <see cref="Dispose" /> methods do nothing.
    /// </remarks>
    public sealed class ExternalSqlSession : ISqlSession
    {
        public ExternalSqlSession(DbConnection connection, DbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        /// <inheritdoc />
        public DbConnection Connection { get; }

        /// <inheritdoc />
        public DbTransaction Transaction { get; }

        /// <inheritdoc />
        public void Complete()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
