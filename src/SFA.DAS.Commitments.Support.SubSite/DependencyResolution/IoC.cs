namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    using SFA.DAS.CommitmentsV2.Data;
    using SFA.DAS.CommitmentsV2.DependencyResolution;
    using StructureMap;

    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<SupportConfigurationRegistry>();
            registry.IncludeRegistry<DatabaseRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();

            registry.IncludeRegistry<AcademicYearDateProviderRegistry>();
            // registry.IncludeRegistry<ApprovalsOuterApiServiceRegistry>();
            //registry.IncludeRegistry<AuthorizationRegistry>();

            registry.IncludeRegistry<ApprenticeshipSearchRegistry>();

            registry.IncludeRegistry<CurrentDateTimeRegistry>();

            registry.IncludeRegistry<DomainServiceRegistry>();

            // registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            //registry.IncludeRegistry<EmployerAccountsRegistry>();
            //registry.IncludeRegistry<FeaturesAuthorizationRegistry>();

            registry.IncludeRegistry<EncodingRegistry>();
            registry.IncludeRegistry<MappingRegistry>();
            registry.IncludeRegistry<MediatorRegistry>();

            //registry.IncludeRegistry<NServiceBusClientUnitOfWorkRegistry>();
            //registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            //registry.IncludeRegistry<ReservationsApiClientRegistry>();

            registry.IncludeRegistry<StateServiceRegistry>();
            registry.IncludeRegistry<CachingRegistry>();

            //registry.IncludeRegistry<ProviderPermissionsAuthorizationRegistry>();
        }
    }
}