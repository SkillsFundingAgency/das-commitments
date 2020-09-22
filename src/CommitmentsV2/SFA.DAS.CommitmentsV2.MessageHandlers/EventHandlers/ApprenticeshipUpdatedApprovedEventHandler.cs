using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipUpdatedApprovedEventHandler : IHandleMessages<ApprenticeshipUpdatedApprovedEvent>
    {
        private readonly ILogger<ApprenticeshipUpdatedApprovedEventHandler> _logger;


        public ApprenticeshipUpdatedApprovedEventHandler(ILogger<ApprenticeshipUpdatedApprovedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ApprenticeshipUpdatedApprovedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"ApprenticeshipUpdatedApprovedEvent received for Apprenticeship {message.ApprenticeshipId}");
            return Task.FromResult(0);
        }
    }
}
