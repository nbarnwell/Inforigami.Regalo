using System;
using System.Collections.Generic;

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

        void IEventHandler<SimpleEventBase>.Handle(SimpleEventBase evt)
        {
            TargetsCalled.Add(typeof(SimpleEventBase));
            MessageTypes.Add(evt.GetType());
        }

        public void Handle(SimpleEvent evt)
        {
            TargetsCalled.Add(typeof(SimpleEvent));
            MessageTypes.Add(evt.GetType());
        }
    }
}