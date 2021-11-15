using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;
using SFA.DAS.PAS.Account.Api.ClientV2.DependencyResolution;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;
using StructureMap;
using EncodingRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.EncodingRegistry;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
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
            registry.IncludeRegistry<PasAccountApiClientRegistry>();
            registry.IncludeRegistry<EncodingRegistry>();
            registry.IncludeRegistry<DiffServiceRegistry>();
            registry.IncludeRegistry<EmployerAccountsRegistry>();
            registry.IncludeRegistry<ReservationsApiClientRegistry>();
            registry.IncludeRegistry<DomainServiceRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
            registry.IncludeRegistry<ApprovalsOuterApiServiceRegistry>();
            registry.IncludeRegistry<LevyTransferMatchingApiClientRegistry>();
        }
    }
}