using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class AcademicYearDateProviderRegistry : Registry
    {
        public AcademicYearDateProviderRegistry()
        {
            For<IAcademicYearDateProvider>().Use<AcademicYearDateProvider>();
        }
    }
}
