using System;
using System.Collections.Generic;
using Inforigami.Regalo.EventSourcing;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class SalesOrder : AggregateRoot
    {
        /*
         * IMPORTANT NOTE!
         * There should normally be no public getters (let alone setters) on AggregateRoot-derived
         * entities. These here are purely because this class is used for unit testing. Normally
         * these should be private fields, and only those required for invariant logic to be
         * validated.
         */
        public readonly IDictionary<string, uint> Products = new Dictionary<string, uint>();
        public bool IsPlaced { get; private set; }

        public void Create(Guid id)
        {
            Record(new SalesOrderCreated(id));
        }

        public void AddLine(string sku, uint quantity)
        {
            Record(new ItemsAddedToOrder(Id, sku, quantity));
        }

        public void PlaceOrder()
        {
            if (false == OrderHasProducts()) throw new InvalidOperationException("Can't place an order with no products.");

            Record(new SalesOrderPlaced(Id));
        }

        private void Apply(SalesOrderCreated evt)
        {
            Id = evt.SalesOrderId;
        }

        private void Apply(ItemsAddedToOrder evt)
        {
            AddProduct(evt.Sku, evt.Quantity);
        }

        private void Apply(SalesOrderPlaced evt)
        {
            IsPlaced = true;
        }

        private bool OrderHasProducts()
        {
            return Products.Count > 0;
        }

        private bool OrderIncludesProduct(string sku)
        {
            return Products.ContainsKey(sku);
        }

        private void AddProduct(string sku, uint quantity)
        {
            if (false == OrderIncludesProduct(sku))
            {
                Products.Add(sku, quantity);
            }
            else
            {
                Products[sku] += quantity;
            }
        }
    }
}
