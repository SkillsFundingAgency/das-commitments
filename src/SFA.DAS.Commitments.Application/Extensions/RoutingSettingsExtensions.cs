using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.Commitments.Application.Extensions
{
    public static class RoutingSettingsExtensions
    {
        private const string CommitmentsV2MessageHandler = "SFA.DAS.CommitmentsV2.MessageHandlers";
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.Application";

        public static void AddRouting(this RoutingSettings routingSettings) 
        {
            routingSettings.RouteToEndpoint(typeof(RunHealthCheckCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(ProviderApproveCohortCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(ProviderSendCohortCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendEmailToEmployerCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(ApproveTransferRequestCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(RejectTransferRequestCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
        }
    }

}
