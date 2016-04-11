using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Command : ICommand
    {
        public Guid MessageId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}