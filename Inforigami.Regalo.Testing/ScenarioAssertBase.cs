using System;
using System.Reflection;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Testing
{
    public abstract class ScenarioAssertBase<THandler, TMessage>
    {
        private readonly THandler _handler;
        private readonly TMessage _message;

        protected ScenarioAssertBase(THandler handler, TMessage message)
        {
            _handler = handler;
            _message = message;
        }

        protected void InvokeHandler()
        {
            Type handlerType = _handler.GetType();
            Type commandType = _message.GetType();

            var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance, null, new[] { commandType }, null);

            if (handleMethod == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        @"Handler is of type {0} and has no public Handle({1}) method. 
Two suggestions:
1) Since often there may be multiple classes representing a message with the same name, be sure to check the handler handles the message in the assembly and namespace you are expecting.
2) Check your Scenario-based test is passing in a message, and not accidentally passing in a Builder object, having forgotten to call .Build().",
                        handlerType,
                        commandType));
            }

            handleMethod.Invoke(_handler, new object[] { _message });
        }
    }
}
