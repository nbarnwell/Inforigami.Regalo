using System;

namespace Inforigami.Regalo.EventSourcing
{
    [Obsolete("Use Inforigami.Regalo.Core.EntityVersion instead.", true)]
    public static class EventStreamVersion
    {
        public const int NoStream = -1;
        public const int Max = -2;

        public static string GetName(int value)
        {
            switch (value)
            {
                case NoStream:
                    return "No-stream";
                case Max:
                    return "Max";
                default:
                    return value.ToString();
            }
        }
    }
}