using Microsoft.Azure;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.Messaging.AzureServiceBus;
using SFA.DAS.Messaging.FileSystem;
using SFA.DAS.Messaging.Interfaces;
using StructureMap;
using System.Configuration;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class EventsUpgradeRegistry : Registry
    {
        public EventsUpgradeRegistry()
        {
            For<IEventConsumer>()
                .Use<EventConsumer>()
                .OnCreation((ctx, publisher) => RegisterEventHandlers(ctx, publisher)).Singleton();

            RegisterMessagePublisher();

            For<IMessagePublisher>().DecorateAllWith<MessagePublisherWithV2Upgrade>();

            For<IEventUpgradeHandler<CohortApprovalRequestedByProvider>>().Use<EventUpgradeHandler>();
        }

        private void RegisterMessagePublisher()
        {
            // The choice of publisher is here in place of the TopicMessagePublisherPolicy.  
            // That had to be removed because it prevents the publisher from being decorated.
            // Also according to the developers of StructureMap it is not considered good practice.
            var environmentName = GetEnvironmentName();
            var messageQueueConnectionString = GetMessageQueueConnectionString(environmentName);
            if (string.IsNullOrEmpty(messageQueueConnectionString))
            {
                For<IMessagePublisher>().Use<FileSystemMessagePublisher>().Ctor<string>().Is(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + $"/{IoC.ServiceName}/");
            }
            else
            {
                For<IMessagePublisher>().Use<TopicMessagePublisher>().Ctor<string>().Is(messageQueueConnectionString);
            }
        }

        private void RegisterEventHandlers(IContext ctx, IEventConsumer eventConsumer)
        {
            eventConsumer.RegisterHandler<CohortApprovalRequestedByProvider>((message) => (ctx.GetInstance<EventUpgradeHandler>() as IEventUpgradeHandler<CohortApprovalRequestedByProvider>).Execute(message));
        }

        private string GetMessageQueueConnectionString(string environment)
        {
            return new ConfigurationService(GetConfigurationRepository(), new ConfigurationOptions(IoC.ServiceName, environment, IoC.ServiceVersion)).Get<CommitmentsApiConfiguration>().MessageServiceBusConnectionString;

        }

        private IConfigurationRepository GetConfigurationRepository()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"] ?? "false"))
            {
                return new FileStorageConfigurationRepository();
            }
            return new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
        }

        private string GetEnvironmentName()
        {
            string text = System.Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(text))
            {
                text = CloudConfigurationManager.GetSetting("EnvironmentName");
            }
            return text;
        }
    }
}