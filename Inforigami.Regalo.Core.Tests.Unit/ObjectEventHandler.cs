using System;
using System.Collections.Generic;
using Inforigami.Regalo.Messaging;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public class ObjectEventHandler
        : IEventHandler<object>,
            IEventHandler<SimpleEventBase>,
            IEventHandler<SimpleEvent>
    {
        public readonly IList<Type> TargetsCalled = new List<Type>();
        public readonly IList<Type> MessageTypes = new List<Type>();

        public void Handle(object evt)
        {
            TargetsCalled.Add(typeof(object));
            MessageTypes.Add(evt.GetType());
        }

        public void Handle(SimpleEvent evt)
        {
            TargetsCalled.Add(typeof(SimpleEvent));
            MessageTypes.Add(evt.GetType());
        }

        public void Handle(SimpleEventBase message)
        {
            TargetsCalled.Add(typeof(SimpleEventBase));
            MessageTypes.Add(message.GetType());
        }
    }
}