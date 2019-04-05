using MediatR;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Configuration;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class TrainingProgrammeRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";

        public TrainingProgrammeRegistry()
        {
            // You'll also need to use the call AddMemoryCache in MVC startup to make IMemoryCache available
            For<IStandardApiClient>().Use<StandardApiClient>().Ctor<string>("baseUrl").Is(ctx => ctx.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl);
            For<IFrameworkApiClient>().Use<FrameworkApiClient>().Ctor<string>("baseUrl").Is(ctx => ctx.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl);

            For<ITrainingProgrammeApiClient>().Use<TrainingProgrammeApiClient>();
        }
    }
}