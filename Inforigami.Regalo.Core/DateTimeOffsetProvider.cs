using System;

namespace Inforigami.Regalo.Core
{
    public static class DateTimeOffsetProvider
    {
        private static readonly Func<DateTimeOffset> __default = () => DateTimeOffset.Now;

        private static Func<DateTimeOffset> _provider = __default;

        public static void Configure(Func<DateTimeOffset> provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _provider = provider;
        }

        public static void Reset()
        {
            Configure(__default);
        }

        public static DateTimeOffset Now()
        {
            return _provider();
        }
    }
}