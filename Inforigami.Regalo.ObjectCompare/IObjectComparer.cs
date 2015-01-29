using System;
using System.Linq.Expressions;

namespace Inforigami.Regalo.ObjectCompare
{
    public interface IObjectComparer
    {
        ObjectComparisonResult AreEqual(object object1, object object2);
        IObjectComparer Ignore<T, TProperty>(Expression<Func<T, TProperty>> ignore);
    }
}
