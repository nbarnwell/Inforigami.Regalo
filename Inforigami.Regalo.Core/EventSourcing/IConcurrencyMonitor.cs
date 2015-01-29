using System.Collections.Generic;

namespace Inforigami.Regalo.Core.EventSourcing
{
    public interface IConcurrencyMonitor
    {
        IEnumerable<ConcurrencyConflict> CheckForConflicts(IEnumerable<object> unseenEvents, IEnumerable<object> uncommittedEvents);
    }
}
