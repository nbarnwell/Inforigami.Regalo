using System.Collections.Generic;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class CommandHandledByMultipleHandlers
    {
        public readonly IList<object> HandlersThatHandledThisMessage = new List<object>();
    }
}