using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.Commitments.Application.Extensions
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(RunHealthCheckCommand), "SFA.DAS.CommitmentsV2.MessageHandlers");
            routingSettings.RouteToEndpoint(typeof(ProviderApproveCohortCommand), "SFA.DAS.CommitmentsV2.MessageHandlers");
        }
    }

}
