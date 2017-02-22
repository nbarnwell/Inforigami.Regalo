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
    public static class Logger
    {
        private static ILogger __defaultLogger;
        private static readonly Func<ILogger> __default = () => GetLogger();

        private static ILogger GetLogger()
        {
            if (__defaultLogger == null)
            {
                __defaultLogger = new ConsoleLogger();
            }

            return __defaultLogger;
        }

        private static Func<ILogger> _provider = __default;

        public static void Configure(Func<ILogger> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _provider = provider;
        }

        public static void Reset()
        {
            Configure(__default);
        }

        public static ILogger Current()
        {
            return _provider();
        }
    }
}