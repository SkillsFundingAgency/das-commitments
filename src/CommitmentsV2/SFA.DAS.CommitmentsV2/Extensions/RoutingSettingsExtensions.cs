using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class RoutingSettingsExtensions
{
    private const string CommitmentsV2MessageHandler = "SFA.DAS.CommitmentsV2.MessageHandlers";
    private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
    private const string ApprenticeCommitmentsJobs = "SFA.DAS.ApprenticeCommitments.Apprenticeship";

    public static void AddRouting(this RoutingSettings routingSettings)
    {
        routingSettings.RouteToCommitmentsMessageHandlers([
            typeof(RunHealthCheckCommand),
            typeof(SendEmailToEmployerCommand),
            typeof(SendEmailToProviderCommand),
            typeof(ApproveTransferRequestCommand),
            typeof(RejectTransferRequestCommand),
            typeof(ApprenticeshipResendInvitationCommand),
            typeof(AutomaticallyStopOverlappingTrainingDateRequestCommand),
            typeof(EmployerSendCohortCommand)
        ]);

        routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
        routingSettings.RouteToEndpoint(typeof(SendApprenticeshipInvitationCommand), ApprenticeCommitmentsJobs);
    }

    private static void RouteToCommitmentsMessageHandlers(this RoutingSettings routingSettings, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            routingSettings.RouteToEndpoint(type, CommitmentsV2MessageHandler);
        }
    }
}