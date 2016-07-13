using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Inforigami.Regalo.EventStore
{
    public class EventStoreSession : IEventStoreSession
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IDictionary<string, EventStoreTransaction> _transactions = new Dictionary<string, EventStoreTransaction>();
        private bool _committed;

        public EventStoreSession(IEventStoreConnection eventStoreConnection)
        {
            if (eventStoreConnection == null) throw new ArgumentNullException(nameof(eventStoreConnection));
            _eventStoreConnection = eventStoreConnection;
        }

        public Task AppendToStreamAsync(string streamId, int expectedVersion, IEnumerable<EventData> events)
        {
            var transaction = GetTransaction(streamId, expectedVersion);
            return transaction.WriteAsync(events);
        }

        public Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string streamId, int start, int count, bool resolveLinkTos)
        {
            return _eventStoreConnection.ReadStreamEventsForwardAsync(streamId, start, count, resolveLinkTos);
        }

        public void Commit()
        {
            _committed = true;
            Task.WaitAll(_transactions.Values.Select(x => x.CommitAsync()).ToArray());
        }

        public void Rollback()
        {
            foreach (var transaction in _transactions.Values)
            {
                transaction.Rollback();
            }
        }

        public void Dispose()
        {
            if (!_committed)
            {
                Rollback();
            }
        }

        private EventStoreTransaction GetTransaction(string streamId, int expectedVersion)
        {
            EventStoreTransaction transaction;
            if (!_transactions.TryGetValue(streamId, out transaction))
            {
                transaction = _eventStoreConnection.StartTransactionAsync(streamId, expectedVersion).Result;
                _transactions.Add(streamId, transaction);
            }

            return transaction;
        }
    }
}