using System.Net.Http;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ApprovalsOuterApiServiceRegistry : Registry
    {
        public ApprovalsOuterApiServiceRegistry()
        {
            For<IApiClient>().Use<ApiClient>().Ctor<HttpClient>().Is(new HttpClient()).Singleton();
            For<ITrainingProgrammeLookup>().Use<TrainingProgrammeLookup>().Singleton();
        }
    }
}