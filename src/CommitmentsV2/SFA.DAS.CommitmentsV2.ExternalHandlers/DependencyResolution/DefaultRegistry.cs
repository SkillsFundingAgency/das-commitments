using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            For<IDbContextFactory>().Use<SynchronizedDbContextFactory>();
            For<ITopicClientFactory>().Use<TopicClientFactory>();
            For<ILegacyTopicMessagePublisher>().Use<LegacyTopicMessagePublisher>().Ctor<string>("connectionString").Is(ctx => ctx.GetInstance<CommitmentsV2Configuration>().MessageServiceBusConnectionString);
            For<IResolveOverlappingTrainingDateRequestService>().Use<ResolveOverlappingTrainingDateRequestService>();
            For<IUlnUtilisationService>().Use<UlnUtilisationService>();
            For<IOverlapCheckService>().Use<OverlapCheckService>();
            For<IEmailOverlapService>().Use<EmailOverlapService>();
        }
    }
}