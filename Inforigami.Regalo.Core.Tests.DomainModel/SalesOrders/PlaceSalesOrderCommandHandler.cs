using System;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class PlaceSalesOrderCommandHandler : ICommandHandler<PlaceSalesOrder>
    {
        private readonly IMessageHandlerContext<SalesOrder> _context;

        public PlaceSalesOrderCommandHandler(IMessageHandlerContext<SalesOrder> context)
        {
            if (context == null) throw new ArgumentNullException("context");
            _context = context;
        }

        public void Handle(PlaceSalesOrder command)
        {
            using (var session = _context.OpenSession(command))
            {
                var order = session.Get(command.SalesOrderId, command.SalesOrderVersion);
                order.PlaceOrder();
                session.SaveAndPublishEvents(order);
            }
        }
    }
}
