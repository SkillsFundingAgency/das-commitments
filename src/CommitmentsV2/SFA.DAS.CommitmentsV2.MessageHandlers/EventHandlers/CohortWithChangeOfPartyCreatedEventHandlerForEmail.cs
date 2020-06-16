using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyCreatedEventHandlerForEmail : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
    {
        public const string TemplateApproveNewEmployerDetailsLevy = "ApproveNewEmployerDetails_Levy";
        public const string TemplateApproveNewEmployerDetailsNonLevy = "ApproveNewEmployerDetails_NonLevy";
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> _logger;

        public CohortWithChangeOfPartyCreatedEventHandlerForEmail(
            IMediator mediator,
            IEncodingService encodingService,
            ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> logger)
        {
            _logger = logger;
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received CohortWithChangeOfPartyCreatedEvent for cohort Id: {message.CohortId}");

            if (message.OriginatingParty != Types.Party.Provider)
            {
                _logger.LogWarning($"CohortWithChangeOfPartyCreatedEvent received with originating party {message.OriginatingParty}");
                return;
            }

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            var tokens = new Dictionary<string, string>
            {
                {"provider_name", cohortSummary.ProviderName },
                {"employer_hashed_account", _encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
                {"cohort_reference", _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
            };

            var templateName = cohortSummary.LevyStatus == Types.ApprenticeshipEmployerType.Levy 
                ? TemplateApproveNewEmployerDetailsLevy
                : TemplateApproveNewEmployerDetailsNonLevy;

            await context.Send(new SendEmailToEmployerCommand(cohortSummary.AccountId,
                templateName,
                tokens));

            _logger.LogInformation($"Sent SendEmailToEmployerCommand with template: {templateName}");

        }
    }
}
