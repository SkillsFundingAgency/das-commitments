using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping;

/// <summary>
///     Base class for testing mappers that do not have a parameterless constructor
/// </summary>
public class OldMapperTester<TFrom, TTo>
    where TFrom : class
    where TTo : class
{
    protected Func<IOldMapper<TFrom, TTo>> MapperCreator { get; set; }

    protected IOldMapper<TFrom, TTo> CreateMapper()
    {
        if (MapperCreator == null)
        {
            throw new InvalidOperationException($"Cannot create a mapper because the {nameof(MapperCreator)} property has not been set");
        }

        return MapperCreator();
    }

    protected async Task AssertPropertySet(Action<TFrom> setInput, Func<TTo, bool> expectOutput)
    {
        var mapper = CreateMapper();

        var input = new Mock<TFrom>();

        setInput(input.Object);

        var output = await mapper.Map(input.Object);

        Assert.That(expectOutput(output), Is.True);
    }

    protected Task AssertPropertySet<TProp, TValue>(Expression<Func<TFrom, TProp>> property, TValue fromValue)
    {
        var (fromProperty, toProperty) = GetSameProperties(property, fromValue);

        return AssertPropertySet(fromProperty, toProperty, fromValue);
    }

    protected Task AssertPropertySet<TProp, TValue>(Expression<Func<TFrom, TProp>> from, Expression<Func<TTo, TProp>> to, TValue fromValue)
    {
        var (fromProperty, toProperty) = GetDifferentProperties(from, to, fromValue);

        return AssertPropertySet(fromProperty, toProperty, fromValue);
    }

    protected async Task AssertPropertySet<TValue>(PropertyInfo fromProperty, PropertyInfo toProperty, TValue fromValue)
    {
        var from = new Mock<TFrom>();

        var mapper = CreateMapper();

        fromProperty.SetValue(from.Object, fromValue);

        var output = await mapper.Map(from.Object);

        var toValue = toProperty.GetValue(output);

        IEqualityComparer comparer = EqualityComparer<TValue>.Default;
                
        var areTheSameValue = comparer.Equals(fromValue, toValue);

        Assert.That(areTheSameValue, Is.True, $"Values not mapped across. Property:{fromProperty.Name} FromValue:{fromValue} ToValue:{toValue}");
    }

    private static PropertyInfo GetPropertyInfo<TType,TProp>(Expression<Func<TType, TProp>> property)
    {
        var memberExpression = property.Body as MemberExpression;

        if (memberExpression == null)
        {
            throw new InvalidOperationException($"The expression is invalid - should be a member expression");
        }

        return memberExpression.Member as PropertyInfo;
    }

    private static (PropertyInfo fromProperty, PropertyInfo toProperty) GetSameProperties<TProp, TValue>(Expression<Func<TFrom, TProp>> property, TValue fromValue)
    {
        var fromProperty = GetPropertyInfo(property);

        CheckSameType(fromProperty, fromValue);

        var toProperty = typeof(TTo).GetProperty(fromProperty.Name);

        if (toProperty == null)
        {
            throw new InvalidOperationException($"The mapped-to type ({typeof(TTo).Name}) does not have a property named '{fromProperty.Name}'");
        }

        return (fromProperty, toProperty);
    }

    private static (PropertyInfo fromProperty, PropertyInfo toProperty) GetDifferentProperties<TProp, TValue>(
        Expression<Func<TFrom, TProp>> from,
        Expression<Func<TTo, TProp>> to,
        TValue fromValue)
    {
        var fromProperty = GetPropertyInfo(from);

        CheckSameType(fromProperty, fromValue);

        var toProperty = GetPropertyInfo(to);

        return (fromProperty, toProperty);
    }

    private static void CheckSameType<TValue>(PropertyInfo property, TValue value)
    {
        if (property.PropertyType != typeof(TValue))
        {
            throw new InvalidOperationException(
                $"'{value?.ToString() ?? "<null>"}' is of type {typeof(TValue).Name} but should be the same type as {property.Name}, which is of type {property.PropertyType.Name}.");
        }
    }
}

/// <summary>
///     Base class for testing mappers that have a parameterless constructor
/// </summary>
public class OldMapperTester<TMapper, TFrom, TTo> : OldMapperTester<TFrom, TTo>
    where TMapper : IOldMapper<TFrom, TTo>, new()
    where TFrom : class
    where TTo : class
{
    public OldMapperTester()
    {
        MapperCreator = () => new TMapper();
    }
}