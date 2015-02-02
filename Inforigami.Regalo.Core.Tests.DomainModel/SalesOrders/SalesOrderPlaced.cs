using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class SalesOrderPlaced : Event
    {
        public Guid Id { get; private set; }

        public SalesOrderPlaced(Guid id)
        {
            Id = id;
        }
    }
}
