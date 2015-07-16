using System;
using System.Collections.Generic;
using System.Linq;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly IDictionary<string, IList<IEvent>> _eventStreams = new Dictionary<string, IList<IEvent>>();

        public IEnumerable<IEvent> GetAllEvents()
        {
            return _eventStreams.Values.SelectMany(x => x);
        }

        public void Save<T>(string aggregateId, int expectedVersion, IEnumerable<IEvent> newEvents)
        {
            if (string.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException("aggregateId");
            if (expectedVersion < 0)                    throw new ArgumentOutOfRangeException("expectedVersion", "Expected version should be greater than or equal to 0.");
            if (newEvents == null)                      throw new ArgumentNullException("newEvents");

            var newEventsList = newEvents.ToList();
            var existingStream = FindEvents(aggregateId);

            CheckConcurrency(expectedVersion, existingStream);

            if (existingStream == null)
            {
                _eventStreams.Add(aggregateId, newEventsList);
            }
            else
            {
                foreach (var evt in newEventsList)
                {
                    existingStream.Add(evt);
                }
            }
        }

        private static void CheckConcurrency(int expectedVersion, ICollection<IEvent> existingStream)
        {
            if (existingStream != null && existingStream.Count != expectedVersion)
            {
                var exception = new EventStoreConcurrencyException(
                    string.Format("Expected version {0} does not match actual version {1}", expectedVersion, existingStream.Count));
                exception.Data.Add("Existing stream", existingStream);
                throw exception;
            }
        }

        public EventStream<T> Load<T>(string aggregateId)
        {
            var events = FindEvents(aggregateId);

            return BuildEventStream<T>(aggregateId, events);
        }

        public EventStream<T> Load<T>(string aggregateId, int version)
        {
            var events = FindEvents(aggregateId, version);

            return BuildEventStream<T>(aggregateId, events);
        }

        private static EventStream<T> BuildEventStream<T>(string aggregateId, IList<IEvent> events)
        {
            if (events == null)
            {
                return null;
            }

            var stream = new EventStream<T>(aggregateId);
            stream.Append(events);
            return stream;
        }

        private IList<IEvent> FindEvents(string id, int? version = null)
        {
            IList<IEvent> events;
            if (!_eventStreams.TryGetValue(id, out events))
            {
                return null;
            }

            if (version.HasValue)
            {
                events = events.Where(x => x.Headers.Version <= version.Value).ToList();
            }

            return events;
        }
    }
}
