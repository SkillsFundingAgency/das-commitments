using System;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipResumedEventHandler : IHandleMessages<ApprenticeshipResumedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipResumedEventHandler> _logger;

        public ApprenticeshipResumedEventHandler(IMediator mediator, ILogger<ApprenticeshipResumedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipResumedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(ApprenticeshipResumedEventHandler)} for apprentice {message?.ApprenticeshipId}");

            var apprenticeshipQueryResult = await _mediator.Send(new GetApprenticeshipQuery(message.ApprenticeshipId));

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeshipQueryResult, message.ResumedOn);

            await context.Send(emailToProviderCommand, new SendOptions());
        }

        private SendEmailToProviderCommand BuildEmailToProviderCommand(GetApprenticeshipQueryResult apprenticeship, DateTime resumeDate)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.ProviderId,
                "ProviderApprenticeshipResumeNotification",
                      new Dictionary<string, string>
                      {
                                  {"EMPLOYER", apprenticeship.EmployerName},
                                  {"APPRENTICE", apprenticeship.FirstName},
                                  {"DATE", resumeDate.ToString("dd/MM/yyyy")},
                                  {"URL", $"{apprenticeship.ProviderId}/apprentices/manage/Hashed/details"}
                      });

            return sendEmailToProviderCommand;
        }
    }
}
