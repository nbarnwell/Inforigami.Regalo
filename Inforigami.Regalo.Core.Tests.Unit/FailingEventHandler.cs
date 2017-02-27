using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class FailingEventHandler : IEventHandler<SimpleEvent>, IEventHandler<IEventHandlingFailedEvent<SimpleEvent>>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();
        public readonly IList<Type> MessageTypes = new List<Type>();

        public void Handle(SimpleEvent evt)
        {
            TargetsCalled.Add(typeof(SimpleEvent));
            MessageTypes.Add(evt.GetType());

            throw new Exception("Deliberate failure.");
        }

        public void Handle(IEventHandlingFailedEvent<SimpleEvent> evt)
        {
            TargetsCalled.Add(typeof(IEventHandlingFailedEvent<SimpleEvent>));
            MessageTypes.Add(evt.GetType());
        }
    }
}