using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
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
