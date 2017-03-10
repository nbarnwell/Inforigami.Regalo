using System;
using Inforigami.Regalo.Interfaces;
using Inforigami.Regalo.ObjectCompare;

namespace Inforigami.Regalo.Testing
{
    public static class ObjectComparerProvider
    {
        private static readonly Func<IObjectComparer> __default =
            () => new ObjectComparer().Ignore<IMessage, Guid>(x => x.MessageId)
                                      .Ignore<IEvent, Guid>(x => x.CausationId)
                                      .Ignore<IEvent, Guid>(x => x.CorrelationId)
                                      .Ignore<IMessage, DateTimeOffset>(x => x.Timestamp)
                                      .Ignore<IMessage, DateTimeOffset>(x => x.CorrelationTimestamp);

        private static Func<IObjectComparer> _provider = __default;

        public static Func<IObjectComparer> Default => __default;

        public static void Configure(Func<IObjectComparer> provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _provider = provider;
        }

        public static void Reset()
        {
            Configure(__default);
        }

        public static IObjectComparer Create()
        {
            return _provider();
        }
    }
}