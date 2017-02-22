using System;

namespace Inforigami.Regalo.Core
{
    public static class Logger
    {
        private static ILogger __defaultLogger;
        private static readonly Func<ILogger> __default = () => GetLogger();

        private static ILogger GetLogger()
        {
            return __defaultLogger ?? (__defaultLogger = new ConsoleLogger());
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