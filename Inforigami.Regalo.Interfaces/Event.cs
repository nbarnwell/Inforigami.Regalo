using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Event : Message, IEvent
    {
        public int Version { get; set; }
    }
}