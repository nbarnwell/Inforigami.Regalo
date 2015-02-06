using System;
using System.Collections.Generic;
using System.Linq;

namespace Inforigami.Regalo.Core
{
    public class TypeInspector
    {
        public IEnumerable<Type> GetTypeHierarchy(Type input)
        {
            // Traverse up the inheritence hierarchy but return
            // in top-down order

            if (input == null) return Enumerable.Empty<Type>();

            var stack = new Stack<Type>();
            AddTypeAndInterfaces(stack, input);

            Type baseType = input;
            while ((baseType = baseType.BaseType) != null)
            {
                AddTypeAndInterfaces(stack, baseType);
            }

            return stack.Distinct().ToList();
        }

        private static void AddTypeAndInterfaces(Stack<Type> stack, Type type)
        {
            // Because we're adding to a stack, the interfaces (more abstract)
            // will actually be returned *after* the implementation (less abstract)
            stack.Push(type);

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
                stack.Push(interfaces[i]);
            }
        }
    }
}
