using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public interface IEventStore
    {
        void Add(Guid aggregateId, IEnumerable<IEvent> events);
        void Update(Guid aggregateId, IEnumerable<IEvent> events);
        IEnumerable<IEvent> Load(Guid aggregateId);
        IEnumerable<IEvent> Load(Guid aggregateId, int version);
    }
}
