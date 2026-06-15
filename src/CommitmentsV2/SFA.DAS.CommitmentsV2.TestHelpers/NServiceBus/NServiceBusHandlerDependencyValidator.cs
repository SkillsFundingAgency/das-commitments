//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace SFA.DAS.CommitmentsV2.TestHelpers.NServiceBus;

//public static class NServiceBusHandlerDependencyValidator
//{
//    private static readonly HashSet<string> ForbiddenDependencyTypeNames = new(StringComparer.Ordinal)
//    {
//        "NServiceBus.IMessageSession",
//        "NServiceBus.IEndpointInstance"
//    };

//    public static IReadOnlyCollection<HandlerDependencyViolation> GetViolations(params Assembly[] assemblies) =>
//        assemblies.SelectMany(GetViolationsFromAssembly).ToList();

//    private static IEnumerable<HandlerDependencyViolation> GetViolationsFromAssembly(Assembly assembly)
//    {
//        foreach (var handlerType in GetHandlerTypes(assembly))
//        {
//            foreach (var constructor in handlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
//            {
//                foreach (var parameter in constructor.GetParameters())
//                {
//                    if (IsForbiddenDependency(parameter.ParameterType))
//                    {
//                        yield return new HandlerDependencyViolation(handlerType, parameter.ParameterType);
//                    }
//                }
//            }
//        }
//    }

//    private static IEnumerable<Type> GetHandlerTypes(Assembly assembly)
//    {
//        Type[] types;

//        try
//        {
//            types = assembly.GetTypes();
//        }
//        catch (ReflectionTypeLoadException ex)
//        {
//            types = ex.Types.Where(type => type is not null).ToArray()!;
//        }

//        return types.Where(IsNServiceBusHandler);
//    }

//    private static bool IsNServiceBusHandler(Type type) =>
//        type is { IsAbstract: false, IsInterface: false } &&
//        type.GetInterfaces().Any(IsIHandleMessagesInterface);

//    private static bool IsIHandleMessagesInterface(Type type) =>
//        type.IsGenericType &&
//        type.GetGenericTypeDefinition().FullName == "NServiceBus.IHandleMessages`1";

//    private static bool IsForbiddenDependency(Type type) =>
//        type.FullName is not null && ForbiddenDependencyTypeNames.Contains(type.FullName);
//}

//public sealed record HandlerDependencyViolation(Type HandlerType, Type ForbiddenDependencyType)
//{
//    public override string ToString() =>
//        $"{HandlerType.FullName} injects {ForbiddenDependencyType.FullName} in its constructor. " +
//        "Use the IMessageHandlerContext parameter in Handle instead.";
//}
