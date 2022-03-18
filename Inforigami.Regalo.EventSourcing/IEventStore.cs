using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IEventStore
    {
        void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents);
        EventStream<T> Load<T>(string aggregateId);
        EventStream<T> Load<T>(string aggregateId, int version);
        [Obsolete("Use Delete<T> instead", true)]
        void Delete(string aggregateId, int expectedVersion);
        void Delete<T>(string aggregateId, int expectedVersion);
        void Flush();
    }
}
