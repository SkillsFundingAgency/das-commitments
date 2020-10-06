using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.PAS.Account.Api.ClientV2;
using SFA.DAS.PAS.Account.Api.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipPausedEventHandler : IHandleMessages<ApprenticeshipPausedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IPasAccountApiClient _pasAccountApiClient;
        private readonly ILogger<ApprenticeshipPausedEventHandler> _logger;

        public ApprenticeshipPausedEventHandler(IMediator mediator, IPasAccountApiClient pasAccountApiClient, ILogger<ApprenticeshipPausedEventHandler> logger)
        {
            _mediator = mediator;
            _pasAccountApiClient = pasAccountApiClient;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipPausedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received {nameof(ApprenticeshipPausedEventHandler)} for apprentice {message?.ApprenticeshipId}");
            var apprenticeshipQueryResult = await _mediator.Send(new GetApprenticeshipQuery(message.ApprenticeshipId));

            var emailToProviderCommand = BuildEmailToProviderCommand(apprenticeshipQueryResult);

            await context.Send(emailToProviderCommand, new SendOptions());
        }

        private SendEmailToProviderCommand BuildEmailToProviderCommand(GetApprenticeshipQueryResult apprenticeship)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.ProviderId,
                "ProviderApprenticeshipPauseNotification",
                      new Dictionary<string, string>
                      {
                                  {"EMPLOYER", apprenticeship.EmployerName},
                                  {"APPRENTICE", apprenticeship.FirstName},
                                  {"DATE", apprenticeship.PauseDate?.ToString("dd/MM/yyyy")},
                                  {"URL", $"{apprenticeship.ProviderId}/apprentices/manage/Hashed/details"}
                      });

            return sendEmailToProviderCommand;
        }
    }
}
