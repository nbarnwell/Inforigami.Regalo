using System;
using System.Data.Common;

namespace Inforigami.Regalo.SqlServer
{
    /// <summary>
    /// Provides access to an already open <see cref="DbConnection" /> and <see cref="DbTransaction" />.
    /// </summary>
    public interface ISqlSession : IDisposable
    {
        /// <summary>
        /// An already open <see cref="DbConnection" /> instance.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// An active <see cref="DbTransaction" /> instance.
        /// </summary>
        DbTransaction Transaction { get; }

        /// <summary>
        /// Allows Regalo to signal successful completion, e.g. to allow the transaction to be committed if appropriate.
        /// </summary>
        void Complete();
    }
}
