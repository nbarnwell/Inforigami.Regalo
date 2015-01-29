using System;
using System.Collections;
using System.Text;

namespace Inforigami.Regalo.Core
{
    public abstract class Message
    {
        protected Message()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}
