using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            registry.IncludeRegistry<MeditorRegistry>();
            registry.IncludeRegistry<DataRegistry>();

            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}
