using System;
using NUnit.Framework;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing.Tests.Unit
{
    [TestFixture]
    public class ApplicationServiceTestingTests : ApplicationServiceTestBase<SalesOrder>
    {
        [SetUp]
        public void SetUp()
        {
            Resolver.Configure(
                type =>
                {
                    if (type == typeof(ILogger))
                    {
                        return new ConsoleLogger();
                    }

                    throw new InvalidOperationException(string.Format("No resolver registered for {0}", type));
                },
                type => null,
                o => { });

            ObjectComparisonResult.ThrowOnFail = true;
        }

        [TearDown]
        public void TearDown()
        {
            ObjectComparisonResult.ThrowOnFail = false;

            Resolver.Reset();
        }

        [Test]
        public void GivenSalesOrderWithSingleOrderLine_WhenPlacingOrder_ThenShouldPlaceOrder()
        {
            Scenario.For<SalesOrder>(Context)
                    .HandledBy<PlaceSalesOrderCommandHandler>(CreateHandler())
                    .Given(SalesOrderTestDataBuilder.NewOrder().WithSingleLineItem().Build())
                    .When(order => new PlaceSalesOrder(order.Id, order.Version))
                    .Then((a, c) => new EventChain(a.BaseVersion) { new SalesOrderPlaced(a.Id) })
                    .Assert();
        }

        [Test]
        public void GivenSalesOrderWithNoLines_WhenPlacingOrder_ThenShouldThrow()
        {
            Scenario.For(Context)
                    .HandledBy(CreateHandler())
                    .Given(SalesOrderTestDataBuilder.NewOrder().Build())
                    .When(order => new PlaceSalesOrder(order.Id, order.Version))
                    .Throws<InvalidOperationException>()
                    .Assert();
        }

        private PlaceSalesOrderCommandHandler CreateHandler()
        {
            return new PlaceSalesOrderCommandHandler(Context);
        }
    }
}
