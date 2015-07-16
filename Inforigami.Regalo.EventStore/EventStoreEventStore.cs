using System;
using System.Collections.Generic;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.EventSourcing;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventStore
{
    public class EventStoreEventStore : IEventStore, IDisposable
    {
        private readonly IEventStoreConnection _eventStoreConnection;

        public EventStoreEventStore(IEventStoreConnection eventStoreConnection)
        {
            if (eventStoreConnection == null) throw new ArgumentNullException("eventStoreConnection");
            _eventStoreConnection = eventStoreConnection;
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            throw new NotImplementedException();

            try
            {
                //string streamId = EventStreamIdFormatter.GetStreamId<T>(aggregateId);

                //_eventStoreConnection.AppendToStreamAsync(streamId, expectedVersion, GetEventData(newEvents));
            }
            catch (WrongExpectedVersionException ex)
            {
                // Wrap in a Regalo-defined exception so that callers don't have to worry what impl is in place
                var concurrencyException = new EventStoreConcurrencyException("Unable to save to EventStore", ex);
                throw concurrencyException;
            }
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            throw new NotImplementedException();
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private string FormatStreamId<T>(string id)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<EventData> GetEventData(IEnumerable<IEvent> newEvents)
        {
            throw new NotImplementedException();
        }
    }
}