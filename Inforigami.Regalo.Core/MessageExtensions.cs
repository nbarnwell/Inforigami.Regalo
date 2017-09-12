using System;
using Inforigami.Regalo.Interfaces;

namespace Inforigami.Regalo.Core
{
    public static class MessageExtensions
    {
        public static TTarget WasCausedBy<TSource, TTarget>(this TTarget target, TSource source)
            where TTarget : IMessage
            where TSource : IMessage
        {
            target.Timestamp += GetTimeSince(source.Timestamp);
            target.CorrelationTimestamp = source.CorrelationTimestamp;
            target.CausationId = source.MessageId;
            target.CorrelationId = source.CorrelationId;
            target.UserId = source.UserId;
            target.Tags = source.Tags;

            return target;
        }

        public static TTarget Causes<TSource, TTarget>(this TSource source, Func<TSource, TTarget> factory)
            where TSource : IMessage
            where TTarget : IMessage
        {
            var msg = factory(source);
            msg.WasCausedBy(source);
            return msg;
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