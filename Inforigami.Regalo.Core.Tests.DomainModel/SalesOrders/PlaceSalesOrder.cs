using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class PlaceSalesOrder : Command
    {
        public Guid SalesOrderId { get; private set; }
        public int SalesOrderVersion { get; private set; }

        public PlaceSalesOrder(Guid salesOrderId, int salesOrderVersion)
        {
            SalesOrderId = salesOrderId;
            SalesOrderVersion = salesOrderVersion;
        }
    }
}
