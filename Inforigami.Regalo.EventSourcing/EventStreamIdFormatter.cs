using System;
using Inforigami.Regalo.Core;

namespace Inforigami.Regalo.EventSourcing
{
    public static class EventStreamIdFormatter
    {
        private static readonly Func<Type, string, string> __default =
            (type, id) => Conventions.StreamIdFormat
                                     .Replace("${aggregateTypeNamespace}", type.Namespace)
                                     .Replace("${aggregateTypeName}", type.Name)
                                     .Replace("${aggregateId}", id);

        private static Func<Type, string, string> _provider = __default;

        public static void Configure(Func<Type, string, string> provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _provider = provider;
        }

        public static void Reset()
        {
            Configure(__default);
        }

        public static string GetStreamId<TAggregateRoot>(string aggregateId)
        {
            return _provider(typeof(TAggregateRoot), aggregateId);
        }
    }
}