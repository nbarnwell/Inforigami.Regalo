using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public abstract class AggregateRoot
    {
        private static readonly ILogger __logger = Resolver.Resolve<ILogger>();
        
        private readonly IDictionary<RuntimeTypeHandle, MethodInfo> _applyMethodCache = new Dictionary<RuntimeTypeHandle, MethodInfo>();
        private readonly IList<IEvent> _uncommittedEvents = new List<IEvent>();

        public Guid Id { get; protected set; }

        /// <summary>
        /// The version of the aggregate when loaded from the store.
        /// </summary>
        public int BaseVersion { get; private set; }

        /// <summary>
        /// The current version of the aggregate taking into account any uncommitted events it has generated.
        /// </summary>
        public int Version { get; private set; }

        public IEnumerable<IEvent> GetUncommittedEvents()
        {
            return _uncommittedEvents.ToList();
        }

        public void AcceptUncommittedEvents()
        {
            if (_uncommittedEvents.Any() == false) return;

            BaseVersion = Version;
            int eventCount = _uncommittedEvents.Count;
            _uncommittedEvents.Clear();
            __logger.Debug(this, "Accepted {0} uncommitted events. Now at base version {1}", eventCount, BaseVersion);
        }

        public void ApplyAll(IEnumerable<IEvent> events)
        {
            var eventList = events as IList<IEvent> ?? events.ToList();

            foreach (var evt in eventList)
            {
                ApplyEvent(evt);
            }

            BaseVersion = Version;

            __logger.Debug(this, "Applied {0} events. Now at base version {1}", eventList.Count, BaseVersion);
        }

        protected void Record(IEvent evt)
        {
            evt.Version = Version + 1;

            ApplyEvent(evt);

            ValidateHasId();

            _uncommittedEvents.Add(evt);

            __logger.Debug(this, "Recorded new event: {0}", evt);
        }

        private void ApplyEvent(IEvent evt)
        {
            Version = evt.Version;

            var applyMethods = FindApplyMethods(evt);

            foreach (var applyMethod in applyMethods)
            {
                applyMethod.Invoke(this, new[] { evt });
            }
        }

        private void ValidateHasId()
        {
            if (Id == default(Guid))
            {
                throw new IdNotSetException();
            }
        }

        /// <summary>
        /// Returns the Apply methods for each type in the event type's inheritance
        /// hierarchy, top-down, from interfaces before classes.
        /// </summary>
        private IEnumerable<MethodInfo> FindApplyMethods(object evt)
        {
            var typeInspector = new TypeInspector();

            var applyMethods = typeInspector.GetTypeHierarchy(evt.GetType())
                                            .Select(FindApplyMethod)
                                            .Where(x => x != null)
                                            .ToList();

            return applyMethods;
        }

        private MethodInfo FindApplyMethod(Type eventType)
        {
            MethodInfo applyMethod;
            if (false == _applyMethodCache.TryGetValue(eventType.TypeHandle, out applyMethod))
            {
                applyMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(m => m.Name == "Apply")
                    .Where(
                        m =>
                        {
                            var parameters = m.GetParameters();
                            return parameters.Length == 1 && parameters[0].ParameterType == eventType;
                        }).SingleOrDefault();

                _applyMethodCache.Add(eventType.TypeHandle, applyMethod);
            }

            if (Conventions.AggregatesMustImplementApplyMethods && applyMethod == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Class {0} does not implement Apply({1} evt). Either implement the method or set Conventions.AggregatesMustImplementApplyMethods to false.",
                        GetType().Name,
                        eventType.Name));
            }

            return applyMethod;
        }
    }
}
