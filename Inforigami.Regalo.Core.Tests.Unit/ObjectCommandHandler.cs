using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class ObjectCommandHandler 
        : ICommandHandler<object>,
            ICommandHandler<SimpleCommandBase>,
            ICommandHandler<SimpleCommand>
    {
        public readonly IList<Type> Messages = new List<Type>(); 

        public void Handle(object command)
        {
            Messages.Add(typeof(object));
        }

        void ICommandHandler<SimpleCommandBase>.Handle(SimpleCommandBase command)
        {
            Messages.Add(typeof(SimpleCommandBase));
        }

        public void Handle(SimpleCommand command)
        {
            Messages.Add(typeof(SimpleCommand));
        }
    }
}