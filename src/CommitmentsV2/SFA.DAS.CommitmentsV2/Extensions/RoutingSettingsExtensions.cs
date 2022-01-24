using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class RoutingSettingsExtensions
    {
        private const string CommitmentsV2MessageHandler = "SFA.DAS.CommitmentsV2.MessageHandlers";
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
        private const string ApprenticeCommitmentsJobs = "SFA.DAS.ApprenticeCommitments.Jobs";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(RunHealthCheckCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendEmailToEmployerCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendEmailToProviderCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(ApproveTransferRequestCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(RejectTransferRequestCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(ApprenticeshipResendInvitationCommand), CommitmentsV2MessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
            routingSettings.RouteToEndpoint(typeof(SendApprenticeshipInvitationCommand), ApprenticeCommitmentsJobs);
        }
    }
}