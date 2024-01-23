using SFA.DAS.Authorization.DependencyResolution.StructureMap;
using SFA.DAS.Authorization.Features.DependencyResolution.StructureMap;
using SFA.DAS.Authorization.ProviderPermissions.DependencyResolution.StructureMap;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.StructureMap;
using StructureMap;
using EncodingRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.EncodingRegistry;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            //registry.IncludeRegistry<AcademicYearDateProviderRegistry>();
            //registry.IncludeRegistry<ApprovalsOuterApiServiceRegistry>();
            //registry.IncludeRegistry<AuthorizationRegistry>();
            //registry.IncludeRegistry<ApprenticeshipSearchRegistry>();
            //registry.IncludeRegistry<ConfigurationRegistry>();
            //registry.IncludeRegistry<CurrentDateTimeRegistry>();
            //registry.IncludeRegistry<DataRegistry>();
            //registry.IncludeRegistry<DomainServiceRegistry>();
            //registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            //registry.IncludeRegistry<EmployerAccountsRegistry>();
            //registry.IncludeRegistry<FeaturesAuthorizationRegistry>();
            //registry.IncludeRegistry<EncodingRegistry>();
            //registry.IncludeRegistry<MappingRegistry>();
            //registry.IncludeRegistry<MediatorRegistry>();
            //registry.IncludeRegistry<NServiceBusClientUnitOfWorkRegistry>();
            //registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            //registry.IncludeRegistry<ReservationsApiClientRegistry>();
            //registry.IncludeRegistry<StateServiceRegistry>();
            //registry.IncludeRegistry<DefaultRegistry>();
            //registry.IncludeRegistry<CachingRegistry>();
            //registry.IncludeRegistry<ProviderPermissionsAuthorizationRegistry>();
        }
    }
}
