using System;

namespace Inforigami.Regalo.Core
{
    public interface IRepository<T>
        where T : AggregateRoot, new()
    {
        T Get(Guid id, int version);
        void Save(T item);
    }
}
