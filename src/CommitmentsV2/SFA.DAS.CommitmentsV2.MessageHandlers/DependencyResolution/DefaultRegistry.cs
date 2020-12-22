using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<IFundingCapService>().Use<FundingCapService>().ContainerScoped();
            For<ITrainingProgrammeLookup>().Use<TrainingProgrammeLookup>().ContainerScoped();
            For<ITopicClientFactory>().Use<TopicClientFactory>();
            For<ILegacyTopicMessagePublisher>().Use<LegacyTopicMessagePublisher>().Ctor<string>("connectionString").Is(ctx=>ctx.GetInstance<CommitmentsV2Configuration>().MessageServiceBusConnectionString);
        }
    }
}