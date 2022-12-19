using System;
using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IEventStore
    {
        void Save<T>(string eventStreamId, int expectedVersion, IEnumerable<IEvent> newEvents);
        EventStream<T> Load<T>(string eventStreamId);
        EventStream<T> Load<T>(string eventStreamId, int version);
        [Obsolete("Use Delete<T> instead", true)]
        void Delete(string eventStreamId, int expectedVersion);
        void Delete<T>(string eventStreamId, int expectedVersion);
        void Flush();
    }
}
