using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.SalesOrders
{
    public class TransactionPlaced : Event
    {
        public string AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public string[] Categories { get; private set; }

        public TransactionPlaced(string accountId, decimal amount, string[] categories)
        {
            AccountId = accountId;
            Amount = amount;
            Categories = categories;
        }
    }
}
