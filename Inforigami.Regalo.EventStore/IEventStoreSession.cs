using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Inforigami.Regalo.EventStore
{
    public interface IEventStoreSession : IDisposable
    {
        Task AppendToStreamAsync(string streamId, int expectedVersion, IEnumerable<EventData> events);
        Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string streamId, int start, int count, bool resolveLinkTos);
        void Commit();
        void Rollback();
    }
}