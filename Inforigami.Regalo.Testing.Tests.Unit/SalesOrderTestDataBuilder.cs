using System;
using Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders;

namespace Inforigami.Regalo.Testing.Tests.Unit
{
    public class SalesOrderTestDataBuilder : TestDataBuilderBase<SalesOrder>
    {
        public SalesOrderTestDataBuilder()
        {
            SetDefaults(() => NewOrder());
        }

        public SalesOrderTestDataBuilder NewOrder()
        {
            AddAction(so => so.Create(Guid.NewGuid()), "New order");
            return this;
        }

        public SalesOrderTestDataBuilder WithSingleLineItem()
        {
            AddAction(so => so.AddLine("SKU", 10), "Order with single line item");
            return this;
        }

        public SalesOrderTestDataBuilder WithDefaults()
        {
            InvokeDefaults();
            return this;
        }

        protected override SalesOrder CreateInstance()
        {
            return new SalesOrder();
        }
    }
}
