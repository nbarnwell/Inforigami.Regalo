using System;

namespace Inforigami.Regalo.RavenDB.Tests.Unit.DomainModel
{
    public class InvariantNotSatisfiedException : Exception
    {
        public InvariantNotSatisfiedException(string message) : base(message)
        {
        }
    }
}
