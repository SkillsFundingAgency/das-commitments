using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.Reservations.Api.Client.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore;
using SFA.DAS.UnitOfWork.NServiceBus;
using SFA.DAS.UnitOfWork.NServiceBus.ClientOutbox;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<AcademicYearDateProviderRegistry>();
            registry.IncludeRegistry<ConfigurationRegistry>();
            registry.IncludeRegistry<CurrentDateTimeRegistry>();
            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<DomainServiceRegistry>();
            registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            registry.IncludeRegistry<HashingRegistry>();
            registry.IncludeRegistry<MappingRegistry>();
            registry.IncludeRegistry<MediatorRegistry>();
            registry.IncludeRegistry<NServiceBusClientUnitOfWorkRegistry>();
            registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            registry.IncludeRegistry<ReservationsApiClientRegistry>();
            registry.IncludeRegistry<TrainingProgrammeRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}
