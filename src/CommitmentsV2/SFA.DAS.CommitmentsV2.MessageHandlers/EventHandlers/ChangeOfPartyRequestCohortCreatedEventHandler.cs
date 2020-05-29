using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ChangeOfPartyRequestCohortCreatedEventHandler : IHandleMessages<ChangeOfPartyRequestCohortCreatedEvent>
    {
        public const string ApproveNewEmployerDetailsLevy = "ApproveNewEmployerDetails_Levy";
        public const string ApproveNewEmployerDetailsNonLevy = "ApproveNewEmployerDetails_NonLevy";
        private readonly IMediator _mediator;
        private readonly ILogger<ChangeOfPartyRequestCohortCreatedEventHandler> _logger;

        public ChangeOfPartyRequestCohortCreatedEventHandler(
            IMediator mediator,
            ILogger<ChangeOfPartyRequestCohortCreatedEventHandler> logger)
        {
            _logger = logger;
            _mediator = mediator;

        }

        public async Task Handle(ChangeOfPartyRequestCohortCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received ChangeOfPartyRequestCohortCreatedEvent for cohort Id: {message.CohortId}");
            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            var tokens = new Dictionary<string, string>
            {
                {"provider_name", cohortSummary.ProviderName }
            };

            var templateName = cohortSummary.LevyStatus == Types.ApprenticeshipEmployerType.Levy 
                ? ApproveNewEmployerDetailsLevy 
                : ApproveNewEmployerDetailsNonLevy;

            await context.Send(new SendEmailToEmployerCommand(cohortSummary.AccountId,
                templateName,
                tokens));

            _logger.LogInformation($"Sent SendEmailToEmployerCommand with template: {templateName}");
        }
    }
}
