using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using ILogger = Inforigami.Regalo.Core.ILogger;

namespace Inforigami.Regalo.EventStore
{
    public class EventStoreSession : IEventStoreSession
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILogger _logger;

        // NOTE: GetEventStore uses a txn per stream written to
        private readonly IDictionary<string, EventStoreTransaction> _transactions = new Dictionary<string, EventStoreTransaction>();
        private bool _committed;

        public EventStoreSession(IEventStoreConnection eventStoreConnection, ILogger logger)
        {
            if (eventStoreConnection == null) throw new ArgumentNullException(nameof(eventStoreConnection));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
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

        public Task<DeleteResult> Delete(string streamId, int expectedVersion)
        {
            return _eventStoreConnection.DeleteStreamAsync(streamId, expectedVersion);
        }

        public void Commit()
        {
            _committed = true;

            _logger.Debug(this, "Committing EventStore transactions for streams: {0}...", string.Join(", ", _transactions.Keys));

            Task.WaitAll(
                _transactions.Values
                             .ToArray()
                             .Select(x => x.CommitAsync())
                             .Cast<Task>()
                             .ToArray());
        }

        public void Rollback()
        {
            _logger.Warn(this, "Rolling-back changes to EventStore...");

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
                _logger.Debug(this, $"Starting new transaction for stream {streamId} at expected version {expectedVersion}");
                transaction = _eventStoreConnection.StartTransactionAsync(streamId, expectedVersion).Result;
                _transactions.Add(streamId, transaction);
            }

            return transaction;
        }
    }
}