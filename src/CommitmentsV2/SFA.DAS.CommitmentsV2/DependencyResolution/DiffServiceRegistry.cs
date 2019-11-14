using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class DiffServiceRegistry : Registry
    {
        public DiffServiceRegistry()
        {
            For<IDiffService>().Use<DiffService>();
        }
    }
}
