using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public static class MessageExtensions
    {
        public static void WasCausedBy(this IMessage source, IMessage target)
        {
            target.Timestamp += GetTimeSince(source.Timestamp);
            target.CorrelationTimestamp = source.CorrelationTimestamp;
            target.CausationId = source.MessageId;
            target.CorrelationId = source.CorrelationId;
            target.UserId = source.UserId;
            target.Tags = source.Tags;
        }
        
        private static TimeSpan GetTimeSince(DateTimeOffset origin)
        {
            var originUtc = origin.ToUniversalTime();
            var nowUtc = DateTimeOffsetProvider.Now().ToUniversalTime();
            var diff = nowUtc - originUtc;

            return diff;
        }
    }
}