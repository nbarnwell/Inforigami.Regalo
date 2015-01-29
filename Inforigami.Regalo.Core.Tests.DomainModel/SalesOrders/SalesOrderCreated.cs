using System;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class SalesOrderCreated : Event
    {
        public Guid AggregateId { get; private set; }

        public SalesOrderCreated(Guid id)
        {
            AggregateId = id;
        }
    }
}
