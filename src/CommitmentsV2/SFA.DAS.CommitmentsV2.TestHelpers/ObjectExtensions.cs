using System;
using System.Linq.Expressions;

namespace SFA.DAS.CommitmentsV2.TestHelpers
{
    public static class ObjectExtensions
    {
        public static void SetValue<T, TProp>(this T o, Expression<Func<T,TProp>> propertySelector, object value)
        {
            var t = o.GetType();
            var body = (MemberExpression)propertySelector.Body;
            t.GetProperty(body.Member.Name).SetValue(o, value, null);
        }

    }
}
