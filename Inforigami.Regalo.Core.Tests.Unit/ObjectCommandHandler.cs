using System;
using System.Collections.Generic;
using Inforigami.Regalo.Messaging;

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

            var updatableMessage = command as CommandHandledByMultipleHandlers;
            if (updatableMessage != null)
            {
                updatableMessage.HandlersThatHandledThisMessage.Add(this);
            }
        }

        public void Handle(SimpleCommand command)
        {
            Messages.Add(typeof(SimpleCommand));
        }

        public void Handle(SimpleCommandBase message)
        {
            Messages.Add(typeof(SimpleCommandBase));
        }
    }
}