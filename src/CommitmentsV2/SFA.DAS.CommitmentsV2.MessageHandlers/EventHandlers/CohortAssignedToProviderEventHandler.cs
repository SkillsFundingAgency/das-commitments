using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class CohortAssignedToProviderEventHandler : IHandleMessages<CohortAssignedToProviderEvent>
    {
        private readonly IMediator _mediator;
        private readonly IApprovalsOuterApiClient _outerApiClient;
        private readonly ILogger<CohortAssignedToProviderEventHandler> _logger;

        public CohortAssignedToProviderEventHandler(IMediator mediator, IApprovalsOuterApiClient outerApiClient, ILogger<CohortAssignedToProviderEventHandler> logger)
        {
            _mediator = mediator;
            _outerApiClient = outerApiClient;
            _logger = logger;
        }

        public async Task Handle(CohortAssignedToProviderEvent message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(CohortAssignedToProviderEvent)} for cohort {message?.CohortId}");
                var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

                if (cohortSummary == null)
                {
                    _logger.LogError($"CohortSummary is null for cohortId {message?.CohortId}");
                    return;
                }
                if (cohortSummary.ChangeOfPartyRequestId.HasValue) return;
               
                var emailRequest = BuildEmailRequest(cohortSummary);
                await SendEmailToAllProviderRecipients(cohortSummary.ProviderId.Value, emailRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Send message to provider for cohort {message?.CohortId} failed");
                throw;
            }
        }

        private Task SendEmailToAllProviderRecipients(long providerId, ProviderEmailRequest request)
        {
            return _outerApiClient.PostWithResponseCode<ProviderEmailRequest, object>(new PostProviderEmailRequest(providerId, request), false);
        }

        private static ProviderEmailRequest BuildEmailRequest(GetCohortSummaryQueryResult cohortSummary)
        {
            var request = new ProviderEmailRequest
            {
                ExplicitEmailAddresses = [],
                Tokens = new Dictionary<string, string>()
            };

            if (!string.IsNullOrWhiteSpace(cohortSummary.LastUpdatedByProviderEmail))
            {
                request.ExplicitEmailAddresses.Add(cohortSummary.LastUpdatedByProviderEmail);
            }

            request.Tokens.Add("cohort_reference", cohortSummary.CohortReference);
            request.Tokens.Add("type", cohortSummary.LastAction == LastAction.Approve ? "approval" : "review");

            if (cohortSummary.IsFundedByTransfer)
            {
                request.TemplateId = "ProviderTransferCommitmentNotification";
                request.Tokens.Add("employer_name", cohortSummary.LegalEntityName);
            }
            else
            {
                request.TemplateId = "ProviderCommitmentNotification";
            }

            return request;
        }
    }

    public class PostProviderEmailRequest : IPostApiRequest<ProviderEmailRequest>
    {
        public string PostUrl => $"/providers/{_providerId}/emails";
        public ProviderEmailRequest Data { get; set; }

        private readonly long _providerId;

        public PostProviderEmailRequest(long providerId, ProviderEmailRequest request)
        {
            _providerId -= providerId;
            Data = request;
        }
    }

    public class ProviderEmailRequest
    {
        public string TemplateId { get; set; }

        public Dictionary<string, string> Tokens { get; set; }

        public List<string> ExplicitEmailAddresses { get; set; }
    }
}