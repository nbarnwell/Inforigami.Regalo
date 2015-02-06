using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class SalesOrderPlaced : Event
    {
        public Guid SalesOrderId { get; private set; }

        public SalesOrderPlaced(Guid salesOrderId)
        {
            SalesOrderId = salesOrderId;
        }
    }
}
