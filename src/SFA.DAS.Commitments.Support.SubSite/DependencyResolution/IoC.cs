namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    using SFA.DAS.Authorization.DependencyResolution.StructureMap;
    using SFA.DAS.CommitmentsV2.Data;
    using SFA.DAS.CommitmentsV2.DependencyResolution;
    using StructureMap;

    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<AuthorizationRegistry>();
            registry.IncludeRegistry<SupportConfigurationRegistry>();
            registry.IncludeRegistry<DatabaseRegistry>();

            registry.IncludeRegistry<DefaultRegistry>();

            //registry.IncludeRegistry<AcademicYearDateProviderRegistry>();
            //registry.IncludeRegistry<CurrentDateTimeRegistry>();
            //registry.IncludeRegistry<DomainServiceRegistry>();
            //registry.IncludeRegistry<EncodingRegistry>();
            //registry.IncludeRegistry<MappingRegistry>();
            //registry.IncludeRegistry<StateServiceRegistry>();
            //registry.IncludeRegistry<CachingRegistry>();
        }
    }
}