using System;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class PlaceSalesOrder : Command
    {
        public PlaceSalesOrder(Guid id)
        {
            Id = id;
        }
    }
}
