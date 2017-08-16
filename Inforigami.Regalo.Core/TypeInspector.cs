using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inforigami.Regalo.Core
{
    public class TypeInspector
    {
        private class QueueOrStack<T> : IEnumerable<T>, ICollection<T>
        {
            private readonly IList<T> _items = new List<T>();
            private readonly TypeHierarchyOrder _typeHierarchyOrder = TypeHierarchyOrder.BottomUp;

            public QueueOrStack()
            {
            }

            public QueueOrStack(TypeHierarchyOrder typeHierarchyOrder)
            {
                _typeHierarchyOrder = typeHierarchyOrder;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(T item)
            {
                if (_typeHierarchyOrder == TypeHierarchyOrder.TopDown)
                {
                    _items.Add(item);
                }
                else
                {
                    _items.Insert(0, item);
                }
            }

            public void Clear()
            {
                _items.Clear();
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return _items.Remove(item);
            }

            public int Count => _items.Count;
            public bool IsReadOnly => _items.IsReadOnly;
        }

        public IEnumerable<Type> GetTypeHierarchy(Type input)
        {
            return GetHierarchy(input);
        }

        public static IEnumerable<Type> GetHierarchy(Type input, TypeHierarchyOrder typeHierarchyOrder = TypeHierarchyOrder.BottomUp)
        {
            // Traverse up the inheritence hierarchy but return
            // in top-down order

            if (input == null) return Enumerable.Empty<Type>();

            var types = new QueueOrStack<Type>(typeHierarchyOrder);
            AddTypeAndInterfaces(types, input);

            Type baseType = input;
            while ((baseType = baseType.BaseType) != null)
            {
                AddTypeAndInterfaces(types, baseType);
            }

            return types.Distinct().ToList();
        }

        private static void AddTypeAndInterfaces(QueueOrStack<Type> types, Type type)
        {
            // Because we're adding to a stack, the interfaces (more abstract)
            // will actually be returned *after* the implementation (less abstract)
            types.Add(type);

            var interfaces = type.GetInterfaces().ToList();
            interfaces.Sort(
                (type1, type2) =>
                {
                    if (type1 == type2)
                    {
                        return string.Compare(type1.FullName, type2.FullName, StringComparison.Ordinal);
                    }

                    return type2.IsAssignableFrom(type1) ? 1 : -1;
                });

            for (int i = interfaces.Count - 1; i >= 0; i--)
            {
                types.Add(interfaces[i]);
            }
        }
    }

    public enum TypeHierarchyOrder
    {
        TopDown,
        BottomUp
    }
}
