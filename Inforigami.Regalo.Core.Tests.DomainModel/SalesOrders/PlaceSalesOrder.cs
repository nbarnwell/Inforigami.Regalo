using System;
using Inforigami.Regalo.Interfaces;

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
