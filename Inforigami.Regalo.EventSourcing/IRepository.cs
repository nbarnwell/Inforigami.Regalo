using System;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IRepository<T>
        where T : AggregateRoot, new()
    {
        T Get(Guid id, int version);
        void Save(T item);
    }
}
