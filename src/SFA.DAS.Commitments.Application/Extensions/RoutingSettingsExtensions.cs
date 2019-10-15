using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.Commitments.Application.Extensions
{
    public static class RoutingSettingsExtensions
    {
        private const string CommitmentsV2MessageHandler = "SFA.DAS.CommitmentsV2.MessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(SendEmailToEmployerCommand), CommitmentsV2MessageHandler);
        }
    }
}