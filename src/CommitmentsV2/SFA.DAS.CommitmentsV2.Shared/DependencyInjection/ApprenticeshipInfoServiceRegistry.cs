using System;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Shared.Configuration;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection
{
    public class ApprenticeshipInfoServiceRegistry : Registry
    {
        public ApprenticeshipInfoServiceRegistry()
        {
            For<IFrameworkApiClient>().Use(ctx => CreateInstance(ctx, CreateFrameworkApiClient)).Singleton();
            For<IStandardApiClient>().Use(ctx => CreateInstance(ctx, CreateStandardApiClient)).Singleton();
            For<ITrainingProgrammeApiClient>().Use<TrainingProgrammeApiClient>().Singleton();
        }

        private T CreateInstance<T>(IContext context, Func<CourseApiClientConfiguration, T> factory)
        {
            var config = context.GetInstance<CourseApiClientConfiguration>();
            return factory(config);
        }

        private FrameworkApiClient CreateFrameworkApiClient(CourseApiClientConfiguration config)
        {
            return new FrameworkApiClient(config.BaseUrl);
        }

        private StandardApiClient CreateStandardApiClient(CourseApiClientConfiguration config)
        {
            return new StandardApiClient(config.BaseUrl);
        }
    }
}