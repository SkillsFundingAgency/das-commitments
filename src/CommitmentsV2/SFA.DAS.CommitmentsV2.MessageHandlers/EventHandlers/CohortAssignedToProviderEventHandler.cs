using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortAssignedToProviderEventHandler(IMediator mediator, IApprovalsOuterApiClient outerApiClient, ILogger<CohortAssignedToProviderEventHandler> logger, 
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<CohortAssignedToProviderEvent>
{
    public async Task Handle(CohortAssignedToProviderEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Received {TypeName} for cohort {Id}", nameof(CohortAssignedToProviderEvent), message?.CohortId);
            if (message != null)
            {
                var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

                if (cohortSummary == null)
                {
                    logger.LogError("CohortSummary is null for cohortId {Id}", message.CohortId);
                    return;
                }

                if (cohortSummary.ChangeOfPartyRequestId.HasValue) return;

                var emailRequest = BuildEmailRequest(cohortSummary, commitmentsV2Configuration.ProviderUrl.ProviderApprenticeshipServiceBaseUrl);
                await SendEmailToAllProviderRecipients(cohortSummary.ProviderId.Value, emailRequest);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Send message to provider for cohort {Id} failed", message?.CohortId);
            throw;
        }
    }

    private async Task SendEmailToAllProviderRecipients(long providerId, ProviderEmailRequest request)
    {
        await outerApiClient.PostWithResponseCode<ProviderEmailRequest, object>(new PostProviderEmailRequest(providerId, request), false);
    }

    private static ProviderEmailRequest BuildEmailRequest(GetCohortSummaryQueryResult cohortSummary, string providerBaseUrl)
    {
        var request = new ProviderEmailRequest
        {
            ExplicitEmailAddresses = [],
            Tokens = new()
        };

        if (!string.IsNullOrWhiteSpace(cohortSummary.LastUpdatedByProviderEmail))
        {
            request.ExplicitEmailAddresses.Add(cohortSummary.LastUpdatedByProviderEmail);
        }

        request.Tokens.Add("cohort_reference", cohortSummary.CohortReference);
        request.Tokens.Add("type", cohortSummary.LastAction == LastAction.Approve ? "approval" : "review");
        request.Tokens.Add("pas_base_url", providerBaseUrl);


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
    public string PostUrl => $"providers/{_providerId}/emails";
    public ProviderEmailRequest Data { get; set; }

    private readonly long _providerId;

    public PostProviderEmailRequest(long providerId, ProviderEmailRequest request)
    {
        _providerId = providerId;
        Data = request;
    }
}

public class ProviderEmailRequest
{
    public string TemplateId { get; set; }

    public Dictionary<string, string> Tokens { get; set; }

    public List<string> ExplicitEmailAddresses { get; set; }
}