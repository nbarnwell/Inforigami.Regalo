using Inforigami.Regalo.Core;

namespace Inforigami.Regalo.Testing
{
    public abstract class AggregateRootTestDataBuilderBase<T> : ITestDataBuilder<T>
        where T : AggregateRoot
    {
        public string CurrentDescription { get; protected set; }

        public T Build()
        {
            var item = BuildAggregate();
            return item;
        }

        protected abstract T BuildAggregate();
    }
}
