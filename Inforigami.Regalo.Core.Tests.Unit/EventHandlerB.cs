using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class EventHandlerB : IEventHandler<EventHandledByMultipleHandlers>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();

        public void Handle(EventHandledByMultipleHandlers evt)
        {
            TargetsCalled.Add(typeof(EventHandledByMultipleHandlers));
        }
    }
}