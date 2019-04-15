using SFA.DAS.CommitmentsV2.DependencyResolution;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<ConfigurationRegistry>();
            registry.IncludeRegistry<HashingRegistry>();
            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<MappingRegistry>();
            registry.IncludeRegistry<MediatorRegistry>();
            registry.IncludeRegistry<TrainingProgrammeRegistry>();
            registry.IncludeRegistry<DomainServiceRegistry>();
            registry.IncludeRegistry<CurrentDateTimeRegistry>();
            registry.IncludeRegistry<AcademicYearDateProviderRegistry>();
        }
    }
}
