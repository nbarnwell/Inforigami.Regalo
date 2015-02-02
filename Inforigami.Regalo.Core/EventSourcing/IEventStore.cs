using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public interface IEventStore
    {
        void Add(Guid aggregateId, IEnumerable<Event> events);
        void Update(Guid aggregateId, IEnumerable<Event> events);
        IEnumerable<Event> Load(Guid aggregateId);
        IEnumerable<Event> Load(Guid aggregateId, int version);
    }
}
