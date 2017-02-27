using System.Collections.Generic;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.EventSourcing
{
    public interface IConcurrencyMonitor
    {
        IEnumerable<ConcurrencyConflict> CheckForConflicts(IEnumerable<IEvent> unseenEvents, IEnumerable<IEvent> uncommittedEvents);
    }
}
