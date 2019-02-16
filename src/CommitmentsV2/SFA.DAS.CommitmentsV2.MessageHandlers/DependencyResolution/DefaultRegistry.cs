using SFA.DAS.CommitmentsV2.Test;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public DefaultRegistry()
        {
            For<Interface1>().Use<Class1>();
        }
    }
}