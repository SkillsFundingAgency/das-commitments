using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;
using StructureMap;
using EncodingRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.EncodingRegistry;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<ConfigurationRegistry>();

            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            registry.IncludeRegistry<MediatorRegistry>();
            registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            registry.IncludeRegistry<EncodingRegistry>();
            registry.IncludeRegistry<DiffServiceRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}