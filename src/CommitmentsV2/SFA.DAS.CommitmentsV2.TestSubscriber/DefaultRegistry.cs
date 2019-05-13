using System;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    internal class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.RegisterConcreteTypesAgainstTheFirstInterface().OnAddedPluginTypes(t => t.Singleton());
                });
        }
    }
}