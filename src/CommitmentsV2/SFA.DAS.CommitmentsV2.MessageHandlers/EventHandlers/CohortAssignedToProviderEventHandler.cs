using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.PAS.Account.Api.ClientV2;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortAssignedToProviderEventHandler : IHandleMessages<CohortAssignedToProviderEvent>
    {
        private readonly IMediator _mediator;
        private readonly IPasAccountApiClient _pasAccountApiClient;
        private readonly ILogger<CohortAssignedToProviderEventHandler> _logger;

        public CohortAssignedToProviderEventHandler(IMediator mediator, IPasAccountApiClient pasAccountApiClient, ILogger<CohortAssignedToProviderEventHandler> logger)
        {
            _mediator = mediator;
            _pasAccountApiClient = pasAccountApiClient;
            _logger = logger;
        }

        public async Task Handle(CohortAssignedToProviderEvent message, IMessageHandlerContext context)
        {
            try
            {
                var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery { CohortId = message.CommitmentId });

                var emailRequest = BuildEmailRequest(cohortSummary);
                await _pasAccountApiClient.SendEmailToAllProviderRecipients(cohortSummary.CohortId, emailRequest)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Send message to provider for cohort {message?.CommitmentId} failed");
                throw;
            }
        }

        private ProviderEmailRequest BuildEmailRequest(GetCohortSummaryQueryResult cohortSummary)
        {
            var request = new ProviderEmailRequest();
            request.ExplicitEmailAddresses = new List<string>();
            request.Tokens = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(cohortSummary.LastUpdatedByProviderEmail))
            {
                request.ExplicitEmailAddresses.Add(cohortSummary.LastUpdatedByProviderEmail
                );
            }

            request.Tokens.Add("cohort_reference", cohortSummary.CohortId.ToString());

            if (cohortSummary.IsFundedByTransfer)
            {
                request.TemplateId = "ProviderTransferCohortNotification";
                request.Tokens.Add("receiving_employer", cohortSummary.LegalEntityName);
            }
            else
            {
                request.TemplateId = "ProviderCohortNotification";
                request.Tokens.Add("type", cohortSummary.LastAction == LastAction.Approve ? "approval" : "review");
            }

            return request;
        }
    }
}