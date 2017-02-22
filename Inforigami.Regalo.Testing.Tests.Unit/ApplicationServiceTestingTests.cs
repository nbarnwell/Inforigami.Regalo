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
                    .Given(SalesOrderTestDataBuilder.WithDefaults().WithSingleLineItem().Build())
                    .When(order => new PlaceSalesOrder(order.Id, order.Version))
                    .Then((a, c) => new EventChain(a.BaseVersion) { new SalesOrderPlaced(a.Id) })
                    .Assert();
        }

        [Test]
        public void GivenNoSalesOrder_WhenCreatingOrder_ThenShouldNotThrow()
        {
            var id = Guid.NewGuid();

            Scenario.For(Context)
                    .HandledBy(new CreateSalesOrderHandler(Context, id))
                    .Given((SalesOrder)null)
                    .When(order => new CreateSalesOrder())
                    .Then((order, cmd) => new EventChain(-1) { new SalesOrderCreated(id) })
                    .Assert();
        }

        [Test]
        public void GivenSalesOrderWithNoLines_WhenPlacingOrder_ThenShouldThrow()
        {
            Scenario.For(Context)
                    .HandledBy(CreateHandler())
                    .Given(SalesOrderTestDataBuilder.WithDefaults().Build())
                    .When(order => new PlaceSalesOrder(order.Id, order.Version))
                    .Throws<InvalidOperationException>()
                    .Assert();
        }

        private PlaceSalesOrderCommandHandler CreateHandler()
        {
            return new PlaceSalesOrderCommandHandler(Context);
        }
    }

    public class CreateSalesOrderHandler : ICommandHandler<CreateSalesOrder>
    {
        private readonly TestingMessageHandlerContext<SalesOrder> _context;
        private readonly Guid _id;

        public CreateSalesOrderHandler(TestingMessageHandlerContext<SalesOrder> context, Guid id)
        {
            _context = context;
            _id = id;
        }

        public void Handle(CreateSalesOrder command)
        {
            var so = new SalesOrder();
            so.Create(_id);
            _context.SaveAndPublishEvents(so);
        }
    }
}
