using System;

namespace Inforigami.Regalo.EventSourcing.Tests.Unit.DomainModel
{
    public class InvariantNotSatisfiedException : Exception
    {
        public InvariantNotSatisfiedException(string message) : base(message)
        {
        }
    }
}
