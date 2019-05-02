using StructureMap;

namespace SFA.DAS.CommitmentsV2.Api.Client.DependencyResolution
{
    public class CommitmentsApiClientRegistry : Registry
    {
        public CommitmentsApiClientRegistry()
        {
            For<ICommitmentsApiClient>().Use(c => c.GetInstance<ICommitmentsApiClientFactory>().CreateClient()).Singleton();
            For<ICommitmentsApiClientFactory>().Use<CommitmentsApiClientFactory>();
        }
    }
}
