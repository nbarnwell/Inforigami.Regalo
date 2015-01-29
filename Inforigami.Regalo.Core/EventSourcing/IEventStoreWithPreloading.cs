using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public interface IEventStoreWithPreloading : IEventStore
    {
        void Preload(IEnumerable<Guid> aggregateIds);
    }
}
