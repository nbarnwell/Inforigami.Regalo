using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public interface IEventStore
    {
        void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents);
        EventStream<T> Load<T>(string aggregateId);
        EventStream<T> Load<T>(string aggregateId, int version);
    }
}
