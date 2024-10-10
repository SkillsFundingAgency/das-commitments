using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyCreatedEventHandlerForEmail(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IMediator mediator,
    IEncodingService encodingService,
    ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> logger,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
{
    public const string TemplateApproveNewEmployerDetailsLevy = "ApproveNewEmployerDetails_Levy";
    public const string TemplateApproveNewEmployerDetailsNonLevy = "ApproveNewEmployerDetails_NonLevy";
    public const string TemplateProviderApprenticeshipChangeOfProviderRequested = "ProviderApprenticeshipChangeOfProviderRequested";

    public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received CohortWithChangeOfPartyCreatedEvent for cohort Id: {CohortId}", message.CohortId);

        var cohortSummary = await mediator.Send(new GetCohortSummaryQuery(message.CohortId));

        switch (message.OriginatingParty)
        {
            case Party.Provider:
                await SendEmployerEmail(context, cohortSummary);
                break;
            case Party.Employer:
                await SendProviderEmail(message, context, cohortSummary);
                break;
        }
    }

    private async Task SendEmployerEmail(IMessageHandlerContext context, GetCohortSummaryQueryResult cohortSummary)
    {
        var tokens = new Dictionary<string, string>
        {
            {"provider_name", cohortSummary.ProviderName },
            {"employer_hashed_account", encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
            {"cohort_reference", encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
        };

        var templateName = cohortSummary.LevyStatus == ApprenticeshipEmployerType.Levy
            ? TemplateApproveNewEmployerDetailsLevy
            : TemplateApproveNewEmployerDetailsNonLevy;

        await context.Send(new SendEmailToEmployerCommand(cohortSummary.AccountId,
            templateName,
            tokens));

        logger.LogInformation("Sent SendEmailToEmployerCommand with template: {TemplateName}", templateName);
    }

    private async Task SendProviderEmail(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context, GetCohortSummaryQueryResult cohortSummary)
    {
        var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

        var apprenticeNamePossessive = await GetApprenticeNamePossessive(changeOfPartyRequest.ApprenticeshipId);            

        var tokens = new Dictionary<string, string>
        {
            { "Subject", $"{cohortSummary.LegalEntityName} {(cohortSummary.IsCompleteForEmployer ? "has added details for you to review" : "has requested that you add details on their behalf")}" },
            { "TrainingProviderName" , cohortSummary.ProviderName},
            { "EmployerName" , cohortSummary.LegalEntityName},
            { "ApprenticeNamePossessive" , apprenticeNamePossessive },
            { "RequestUrl", $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{cohortSummary.ProviderId}/unapproved/{cohortSummary.CohortReference}/details" }
        };

        await context.Send(new SendEmailToProviderCommand(cohortSummary.ProviderId.Value, TemplateProviderApprenticeshipChangeOfProviderRequested, tokens));

        logger.LogInformation($"Sent SendEmailToProviderCommand with template: ProviderApprenticeshipChangeOfProviderRequested");
    }

    private async Task<string> GetApprenticeNamePossessive(long apprenticeshipId)
    {
        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, default);

        return apprenticeship.ApprenticeName.EndsWith('s') ? apprenticeship.ApprenticeName + "'" : apprenticeship.ApprenticeName + "'s";
    }
}