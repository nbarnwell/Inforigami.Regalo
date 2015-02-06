using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class ItemsAddedToOrder : Event
    {
        public Guid OrderId { get; private set; }
        public string Sku { get; private set; }
        public uint Quantity { get; private set; }

        public ItemsAddedToOrder(Guid orderId, string sku, uint quantity)
        {
            OrderId = orderId;
            Sku = sku;
            Quantity = quantity;
        }
    }
}
