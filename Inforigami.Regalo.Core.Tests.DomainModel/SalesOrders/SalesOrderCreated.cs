using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class SalesOrderCreated : Event
    {
        public Guid SalesOrderId { get; private set; }

        public SalesOrderCreated(Guid salesOrderId)
        {
            SalesOrderId = salesOrderId;
        }
    }
}
