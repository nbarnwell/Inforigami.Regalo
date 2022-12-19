using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IEventStoreWithPreloading : IEventStore
    {
        void Preload(IEnumerable<string> aggregateIds);
    }
}
