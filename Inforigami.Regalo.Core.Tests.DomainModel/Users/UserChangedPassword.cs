using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.Tests.DomainModel.Users
{
    public class UserChangedPassword : Event
    {
        public string NewPassword { get; private set; }

        public UserChangedPassword(string newpassword)
        {
            if (newpassword == null) throw new ArgumentNullException("newpassword");

            NewPassword = newpassword;
        }
    }
}
