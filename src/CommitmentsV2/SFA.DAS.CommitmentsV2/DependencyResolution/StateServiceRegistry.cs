using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class StateServiceRegistry : Registry
    {
        public StateServiceRegistry()
        {
            For<IStateService>().Use<StateService>();
        }
    }
}
