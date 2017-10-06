using System;

namespace Inforigami.Regalo.SqlServer.ChangeDataCapture
{
    public class RowUpdated<TRow> : Change<TRow>
    {
        public TRow Previous { get; }
        public TRow Current { get; }

        public RowUpdated(TRow previous, TRow current)
        {
            if (previous == null) throw new ArgumentNullException(nameof(previous));
            if (current == null) throw new ArgumentNullException(nameof(current));

            Previous = previous;
            Current = current;
        }
    }
}