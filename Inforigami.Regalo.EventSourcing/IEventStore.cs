using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IEventStore
    {
        void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents);
        EventStream<T> Load<T>(string aggregateId);
        EventStream<T> Load<T>(string aggregateId, int version);
        void Delete(string aggregateId, int version);
    }
}
