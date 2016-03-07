using System;

namespace Inforigami.Regalo.Testing
{
    public interface IGivenSetter<TEntity, THandler>
    {
        [Obsolete("Use the overload that takes an entity instead.")]
        IWhenSetter<TEntity, THandler> Given(ITestDataBuilder<TEntity> testDataBuilder);
        IWhenSetter<TEntity, THandler> Given(TEntity entity);
    }
}
