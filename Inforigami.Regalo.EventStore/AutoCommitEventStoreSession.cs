using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Inforigami.Regalo.EventStore
{
    public class AutoCommitEventStoreSession : IEventStoreSession
    {
        private readonly IEventStoreConnection _eventStoreConnection;

        public AutoCommitEventStoreSession(IEventStoreConnection eventStoreConnection)
        {
            if (eventStoreConnection == null) throw new ArgumentNullException(nameof(eventStoreConnection));
            _eventStoreConnection = eventStoreConnection;
        }

        public Task AppendToStreamAsync(string streamId, int expectedVersion, IEnumerable<EventData> events)
        {
            return _eventStoreConnection.AppendToStreamAsync(streamId, expectedVersion, events);
        }

        public Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string streamId, int start, int count, bool resolveLinkTos)
        {
            return _eventStoreConnection.ReadStreamEventsForwardAsync(streamId, start, count, resolveLinkTos);
        }

        public void Commit()
        {
            // Nothing to do
        }

        public void Rollback()
        {
            // Nothing to do
        }

        public void Dispose()
        {
            // Nothing to do
        }
    }
}