using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortWithChangeOfPartyCreatedEventHandlerForEmail : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
    {
        public const string TemplateApproveNewEmployerDetailsLevy = "ApproveNewEmployerDetails_Levy";
        public const string TemplateApproveNewEmployerDetailsNonLevy = "ApproveNewEmployerDetails_NonLevy";

        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> _logger;

        public CohortWithChangeOfPartyCreatedEventHandlerForEmail(Lazy<ProviderCommitmentsDbContext> dbContext,
            IMediator mediator,
            IEncodingService encodingService,
            ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mediator = mediator;
            _encodingService = encodingService;
        }

        public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Received CohortWithChangeOfPartyCreatedEvent for cohort Id: {message.CohortId}");

            var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

            if (message.OriginatingParty == Party.Provider)
            {
                await SendEmployerEmail(context, cohortSummary);
            }
            else if (message.OriginatingParty == Party.Employer)
            {
                await SendProviderEmail(message, context, cohortSummary);
            }
        }

        private async Task SendEmployerEmail(IMessageHandlerContext context, GetCohortSummaryQueryResult cohortSummary)
        {
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

        private async Task SendProviderEmail(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context, GetCohortSummaryQueryResult cohortSummary)
        {
            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
            
            var apprenticeNamePossessive = await GetApprenticeNamePossessive(changeOfPartyRequest.ApprenticeshipId);

            var tokens = new Dictionary<string, string>
                {
                    { "Subject", $"{cohortSummary.LegalEntityName} {(cohortSummary.IsCompleteForEmployer ? "has added details for you to review" : "has requested that you add details on their behalf")}" },
                    { "TrainingProviderName" , cohortSummary.ProviderName},
                    { "EmployerName" , cohortSummary.LegalEntityName},
                    { "ApprenticeNamePossessive" , apprenticeNamePossessive },
                    { "RequestUrl", $"{cohortSummary.ProviderId}/apprentices/{cohortSummary.CohortReference}/details" }
                };

            await context.Send(new SendEmailToProviderCommand(cohortSummary.ProviderId.Value, "ProviderApprenticeshipChangeOfProviderRequested", tokens));

            _logger.LogInformation($"Sent SendEmailToProviderCommand with template: ProviderApprenticeshipChangeOfProviderRequested");
        }

        private async Task<string> GetApprenticeNamePossessive(long apprenticeshipId)
        {
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, default);

            return apprenticeship.ApprenticeName.EndsWith("s") ? apprenticeship.ApprenticeName + "'" : apprenticeship.ApprenticeName + "'s";
        }
    }
}
