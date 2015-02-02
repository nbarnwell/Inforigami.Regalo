using System;

namespace Inforigami.Regalo.Core
{
    public static class GuidProvider
    {
        private static readonly Func<Guid> __default = () => Guid.NewGuid();

        private static Func<Guid> _provider = __default;

        public static void Configure(Func<Guid> provider)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _provider = provider;
        }

        public static void Reset()
        {
            Configure(__default);
        }

        public static Guid NewGuid()
        {
            return _provider();
        }
    }
}