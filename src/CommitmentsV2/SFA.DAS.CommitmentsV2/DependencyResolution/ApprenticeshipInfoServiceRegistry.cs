using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.Providers.Api.Client;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ApprenticeshipInfoServiceRegistry : Registry
    {
        public ApprenticeshipInfoServiceRegistry()
        {
            For<IFrameworkApiClient>().Use<FrameworkApiClient>().Ctor<string>("baseUrl").Is(ctx => ctx.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl);
            For<IProviderApiClient>().Use<ProviderApiClient>().Ctor<string>("baseUrl").Is(ctx => ctx.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl);
            For<IStandardApiClient>().Use<StandardApiClient>().Ctor<string>("baseUrl").Is(ctx => ctx.GetInstance<ApprenticeshipInfoServiceConfiguration>().BaseUrl);
            For<ITrainingProgrammeApiClient>().Use<TrainingProgrammeApiClient>().Singleton();
            For<ITrainingProgrammeLookup>().Use<TrainingProgrammeLookup>().Singleton();
        }
    }
}