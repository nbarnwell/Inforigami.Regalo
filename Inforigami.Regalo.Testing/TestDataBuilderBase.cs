using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Testing
{
    /// <summary>Useful base class for TestDataBuilders.</summary>
    /// <typeparam name="T">The type of object the test data builder is to create.</typeparam>
    /// <remarks>Inherit from this class, then, to your subclass, add public methods that
    /// return the databuilder (i.e. <code>return this;</code>), such that they can be used in the method-chaining style.<br />Each 
    /// of those methods should call <code>AddAction()</code> with a delegate that will be queued-up
    /// and used to build the finished object when <code>Build()</code> is called.</remarks>
    /// <example><code>
    /// public SalesOrderTestDataBuilder NewOrder()
    /// {
    ///     AddAction(so => so.Create(Guid.NewGuid()), "New order");
    ///     return this;
    /// }
    /// </code></example>
    public abstract class TestDataBuilderBase<T> : ITestDataBuilder<T>
    {
        private readonly IList<Action<T>> _actions = new List<Action<T>>();

        public string CurrentDescription { get; private set; }
        
        public T Build()
        {
            var aggregate = CreateInstance();

            foreach (var action in _actions)
            {
                action.Invoke(aggregate);
            }

            return aggregate;
        }

        /// <summary>
        /// Each method on your TestDataBuilder should only call AddAction(), as the builder
        /// will be responsible for remembering these actions, and executing them as needed
        /// when Build() is called.
        /// </summary>
        /// <param name="action">The action to queue for modifying the resulting object.</param>
        /// <param name="newDescription">A description of the state of the object once this action has been performed.</param>
        protected void AddAction(Action<T> action, string newDescription)
        {
            _actions.Add(action);
            CurrentDescription = newDescription;
        }

        protected abstract T CreateInstance();
    }
}
