using System;

namespace Inforigami.Regalo.Interfaces
{
    public interface IEvent : IMessage
    {
        int Version { get; set; }
    }
}
