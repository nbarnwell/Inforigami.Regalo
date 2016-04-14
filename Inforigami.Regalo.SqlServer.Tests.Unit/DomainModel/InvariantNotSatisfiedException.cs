using System;

namespace Inforigami.Regalo.SqlServer.Tests.Unit.DomainModel
{
    public class InvariantNotSatisfiedException : Exception
    {
        public InvariantNotSatisfiedException(string message) : base(message)
        {
        }
    }
}
