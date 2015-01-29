using System;

namespace Inforigami.Regalo.Core.Tests.DomainModel.Users
{
    public class UserRegistered : Event
    {
        public UserRegistered(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
    }
}
