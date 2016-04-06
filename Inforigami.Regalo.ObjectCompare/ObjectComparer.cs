using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Inforigami.Regalo.ObjectCompare
{
    public class ObjectComparer : IObjectComparer
    {
        private readonly Stack<string> _propertyComparisonStack = new Stack<string>();
        private readonly IList<object> _circularReferenceChecklist = new List<object>();
        private readonly PropertyComparisonIgnoreList _ignores = new PropertyComparisonIgnoreList();

        public ObjectComparisonResult AreEqual(object expected, object actual)
        {
            if (expected == null && actual == null)
            {
                return ObjectComparisonResult.Success();
            }

            var type = (expected ?? actual).GetType();
            if (_propertyComparisonStack.Count == 0) _propertyComparisonStack.Push(type.Name);

            if ((expected == null) != (actual == null))
            {
                return ObjectComparisonResult.Fail(_propertyComparisonStack, "Nullity differs. expected is {0} while actual is {1}.", expected ?? "null", actual ?? "null");
            }

            var expectedType = expected.GetType();
            var actualType = actual.GetType();

            if (expectedType != actualType)
            {
                // Let the type check go if they're both at least enumerable
                if (false == (expected.GetType().IsEnumerable() && actual.GetType().IsEnumerable()))
                {
                    return ObjectComparisonResult.Fail(_propertyComparisonStack, "Objects are of different type. expected is {0} while actual is {1}.", expectedType, actualType);
                }
            }

            if (expectedType.IsPrimitive())
            {
                return ArePrimitivesEqual(expected, actual);
            }

            if (expectedType.IsEnumerable())
            {
                return AreObjectsInEnumerablesEqual(expected, actual);
            }

            return AreComplexObjectsEqual(expected, actual);
        }

        public IObjectComparer Ignore<T, TProperty>(Expression<Func<T, TProperty>> ignore)
        {
            _ignores.Add(ignore);
            return this;
        }

        private ObjectComparisonResult ArePrimitivesEqual(object expected, object actual)
        {
            if (!actual.Equals(expected))
            {
                return ObjectComparisonResult.Fail(_propertyComparisonStack, "Primitive values differ. expected: {0}, actual: {1}.", expected, actual);
            }

            return ObjectComparisonResult.Success();
        }

        private ObjectComparisonResult AreComplexObjectsEqual(object expected, object actual)
        {
            var typeBeingCompared = expected.GetType();
            var properties = typeBeingCompared.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (false == properties.Any())
            {
                return ObjectComparisonResult.Success();
            }

            foreach (var property in properties)
            {
                if (false == _ignores.Contains(typeBeingCompared, property.Name))
                {
                    _propertyComparisonStack.Push(property.Name);
                    try
                    {
                        var value1 = property.GetValue(expected, null);

                        if (property.PropertyType.IsPrimitive == false)
                        {
                            if (_circularReferenceChecklist.Contains(value1))
                            {
                                return ObjectComparisonResult.Success();
                            }

                            _circularReferenceChecklist.Add(value1);
                        }

                        var value2 = property.GetValue(actual, null);

                        // NOTE: Recursion
                        var result = AreEqual(value1, value2);

                        if (!result.AreEqual)
                        {
                            return result;
                        }
                    }
                    finally
                    {
                        _propertyComparisonStack.Pop();
                    }
                }
            }

            return ObjectComparisonResult.Success();
        }

        private ObjectComparisonResult AreObjectsInEnumerablesEqual(object value1, object value2)
        {
            var enumerator1 = ((IEnumerable)value1).GetEnumerator();
            var enumerator2 = ((IEnumerable)value2).GetEnumerator();

            bool hasNext1 = enumerator1.MoveNext();
            bool hasNext2 = enumerator2.MoveNext();

            while (hasNext1 && hasNext2)
            {
                var result = AreEqual(enumerator1.Current, enumerator2.Current);
                if (!result.AreEqual)
                {
                    return result;
                }

                hasNext1 = enumerator1.MoveNext();
                hasNext2 = enumerator2.MoveNext();
            }

            if (hasNext1 != hasNext2)
            {
                var r = new EnumerableComparisonReport();
                var report = r.Generate("Expected", value1, "Actual", value2);
                return ObjectComparisonResult.Fail(_propertyComparisonStack, "Enumerable properties have different lengths:\r\n" + report);
            }

            return ObjectComparisonResult.Success();
        }

    }
}
