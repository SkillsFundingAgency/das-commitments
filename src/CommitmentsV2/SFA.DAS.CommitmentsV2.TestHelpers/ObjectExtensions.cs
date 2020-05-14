using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SFA.DAS.CommitmentsV2.TestHelpers
{
    public static class ObjectExtensions
    {
        public static void SetValue<T, TProp>(this T o, Expression<Func<T,TProp>> propertySelector, object value)
        {
            var t = o.GetType();
            var body = (MemberExpression)propertySelector.Body;

            var property = t.GetProperty(body.Member.Name);
            if (property != null && property.CanWrite)
            {
                property.SetValue(o, value, null);
                return;
            }

            var field = t.GetField(body.Member.Name);
            if (field != null)
            {
                field.SetValue(o, value);
                return;
            }

            var backingField = t.GetField($"<{body.Member.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(o, value);
                return;
            }
            
            throw new InvalidOperationException($"Unable to set {body.Member.Name} on {t.Name}");
        }
    }
}
