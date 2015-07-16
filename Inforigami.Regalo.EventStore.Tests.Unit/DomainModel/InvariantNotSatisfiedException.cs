using System;

namespace Inforigami.Regalo.EventStore.Tests.Unit.DomainModel
{
    public class InvariantNotSatisfiedException : Exception
    {
        public InvariantNotSatisfiedException(string message) : base(message)
        {
        }
    }
}
