using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.PAS.Account.Api.ClientV2.DependencyResolution;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;
using StructureMap;
using ApprenticeshipInfoServiceRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.ApprenticeshipInfoServiceRegistry;
using EncodingRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.EncodingRegistry;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<ConfigurationRegistry>();
            registry.IncludeRegistry<ApprenticeshipInfoServiceRegistry>();
            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            registry.IncludeRegistry<MediatorRegistry>();
            registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            registry.IncludeRegistry<PasAccountApiClientRegistry>();
            registry.IncludeRegistry<EncodingRegistry>();
            registry.IncludeRegistry<DiffServiceRegistry>();
            registry.IncludeRegistry<EmployerAccountsRegistry>();
            registry.IncludeRegistry<ReservationsApiClientRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}