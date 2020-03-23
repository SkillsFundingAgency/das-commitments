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

            For<IEventUpgradeHandler<CohortApprovalRequestedByProvider>>().Use<EventUpgradeHandler>().Singleton();
        }

        private void RegisterMessagePublisher()
        {
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
            return new AzureTableStorageConfigurationRepository(ConfigurationManager.AppSettings["ConfigurationStorageConnectionString"]);
        }

        private string GetEnvironmentName()
        {
            string text = System.Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(text))
            {
                text = ConfigurationManager.AppSettings["EnvironmentName"];
            }
            return text;
        }
    }
}