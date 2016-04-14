using System;
using System.Collections.Generic;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.SqlServer.Tests.Unit.DomainModel.Customers
{
    public class Customer : AggregateRoot
    {
        private readonly ISet<string> _subscribedNewsletters = new HashSet<string>(); 

        public void Signup()
        {
            Record(new CustomerSignedUp(Guid.NewGuid()));
        }

        public void SubscribeToNewsletter(string newsletterName)
        {
            if (!string.IsNullOrWhiteSpace(newsletterName)) throw new InvalidOperationException("Newsletter name is not suitable for subscription.");
            if (_subscribedNewsletters.Contains(newsletterName)) throw new InvalidOperationException("Customer is already subscribed to this newsletter.");

            Record(new SubscribedToNewsletter(newsletterName));
        }

        public void AssignAccountManager(Guid accountManagerId, DateTime startDate)
        {
            if (startDate > DateTime.Today) throw new InvariantNotSatisfiedException("Cannot assign an account manager whose start date is in the future.");

            Record(new AccountManagerAssigned(accountManagerId));
        }

        private void Apply(CustomerSignedUp evt)
        {
            Id = evt.AggregateId;
        }

        private void Apply(SubscribedToNewsletter evt)
        {
            _subscribedNewsletters.Add(evt.NewsletterName);
        }
    }

    public class AccountManagerAssigned : Event
    {
        public Guid AccountManagerId { get; private set; }

        public AccountManagerAssigned(Guid accountManagerId)
        {
            AccountManagerId = accountManagerId;
        }
    }
}
