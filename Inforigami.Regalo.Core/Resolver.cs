using System;
using System.Collections.Generic;

namespace Inforigami.Regalo.Core
{
    public static class Resolver
    {
        private static Func<Type, object> _singleResolver;
        private static Func<Type, IEnumerable<object>> _multipleResolver;
        private static Action<object> _releaser;

        public static void Configure(
            Func<Type, object> singleResolver,
            Func<Type, IEnumerable<object>> multipleResolver,
            Action<object> releaser)
        {
            if (singleResolver == null) throw new ArgumentNullException("singleResolver");
            if (multipleResolver == null) throw new ArgumentNullException("multipleResolver");
            if (releaser == null) throw new ArgumentNullException("releaser");

            if (_singleResolver != null || _multipleResolver != null || _releaser != null)
            {
                throw new InvalidOperationException(
                    "Resolvers have already been set. Be sure to call DependencyResolver.Reset() first if you deliberately wish to change the implementation.");
            }

            _singleResolver = singleResolver;
            _multipleResolver = multipleResolver;
            _releaser = releaser;
        }

        public static void Reset()
        {
            _singleResolver = null;
            _multipleResolver = null;
            _releaser = null;
        }

        public static T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public static object Resolve(Type type)
        {
            if (_singleResolver == null)
            {
                throw new InvalidOperationException(
                    "Resolvers have not been set. Be sure to call DependencyResolver.Configure() in your application initialisation.");
            }

            return _singleResolver.Invoke(type);
        }

        public static IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)ResolveAll(typeof(T));
        }

        public static IEnumerable<object> ResolveAll(Type type)
        {
            if (_multipleResolver == null)
            {
                throw new InvalidOperationException("Resolvers have not been set. Be sure to call DependencyResolver.Configure() in your application initialisation.");
            }

            return _multipleResolver.Invoke(type);
        }

        public static void Release(object component)
        {
            if (_releaser == null)
            {
                throw new InvalidOperationException("Resolvers have not been set. Be sure to call Regalo.Core.Resolver.Configure() in your application initialisation.");
            }

            _releaser(component);
        }
    }
}
