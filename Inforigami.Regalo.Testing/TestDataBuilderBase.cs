using System;
using System.Collections.Generic;
using System.Linq;

namespace Inforigami.Regalo.Testing
{
    /// <summary>Useful base class for TestDataBuilders.</summary>
    /// <typeparam name="T">The type of object the test data builder is to create.</typeparam>
    /// <remarks>To use this class:
    /// <list type="number">
    /// <item><description>Create a class deriving from TestDataBuilderBase&lt;T&gt;, where T is the class of object to be built.</description></item>
    /// <item><description></description></item>
    /// <item><description>Create as many <code>public [Type][Property] { get; private set; }</code> properties as necessary for instances of T to be built/configured.</description></item>
    /// <item><description>Note these are "public get" properties so that tests can access the values (even after they are customised) directly in assertions.</description></item>
    /// <item><description>Implement the CreateInstance() abstract method to do nothing more than is required to create a new instance of T.
    /// Note: This is to support the case where T has no default constructor, as opposed to using generic constraints to make a default constructor obligatory.</description></item>
    /// <item><description>Create `public static [nameof(T)] Builder WithDefaults()` that returns a builder preconfigured in a default way.</description></item>
    /// <item><description>Create methods to configure the instance of the builder either by assigning new values to the properties that will be used to build the object, or by calling `AddAction()` to build up a "script" that will be used to build the object.</description></item>
    /// </list>
    /// </remarks>
    /// <example><code>
    /// public SalesOrderTestDataBuilder NewOrder()
    /// {
    ///     AddAction(so => so.Create(Guid.NewGuid()), "New order");
    ///     return this;
    /// }
    /// </code></example>
    public abstract class TestDataBuilderBase<T> : ITestDataBuilder<T>
        where T: class
    {
        private readonly IList<Action<T>> _actions = new List<Action<T>>();
        private Action _defaults;

        public string CurrentDescription { get; private set; }

        public T Build()
        {
            var aggregate = CreateInstance();

            if (!_actions.Any())
            {
                _defaults?.Invoke();
            }

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
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            CurrentDescription = newDescription;
        }

        /// <summary>
        /// If your object requires at least default values, even if no AddAction() calls are made,
        /// set them by calling SetDefaults() in the ctor.
        /// </summary>
        /// <param name="action">The action to queue for modifying the resulting object.</param>
        protected void SetDefaults(Action action)
        {
            _defaults = action;
        }

        /// <summary>
        /// Since a base class can't "return this" to return the subclass, this method makes it 
        /// simple to implement a "WithDefaults" method on overriding classes, which should
        /// simply call <code>base.InvokeDefaults(); return this;</code>. This allows for a call
        /// to <code>WithDefaults()</code> to be chained with other customisation methods.
        /// </summary>
        protected void InvokeDefaults()
        {
            _defaults?.Invoke();
        }

        /// <summary>
        /// Creates the instance to be returned, just before all the actions are applied.
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateInstance();

        public static T None()
        {
            return null;
        }
    }
}
