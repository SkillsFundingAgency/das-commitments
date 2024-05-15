using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortWithChangeOfPartyCreatedEventHandlerForEmail : IHandleMessages<CohortWithChangeOfPartyCreatedEvent>
{
    public const string TemplateApproveNewEmployerDetailsLevy = "ApproveNewEmployerDetails_Levy";
    public const string TemplateApproveNewEmployerDetailsNonLevy = "ApproveNewEmployerDetails_NonLevy";
    public const string TemplateProviderApprenticeshipChangeOfProviderRequested = "ProviderApprenticeshipChangeOfProviderRequested";

    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly IMediator _mediator;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> _logger;
    private readonly CommitmentsV2Configuration _commitmentsV2Configuration;

    public CohortWithChangeOfPartyCreatedEventHandlerForEmail(Lazy<ProviderCommitmentsDbContext> dbContext,
        IMediator mediator,
        IEncodingService encodingService,
        ILogger<CohortWithChangeOfPartyCreatedEventHandlerForEmail> logger,
        CommitmentsV2Configuration commitmentsV2Configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mediator = mediator;
        _encodingService = encodingService;
        _commitmentsV2Configuration = commitmentsV2Configuration;
    }

    public async Task Handle(CohortWithChangeOfPartyCreatedEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Received CohortWithChangeOfPartyCreatedEvent for cohort Id: {CohortId}", message.CohortId);

        var cohortSummary = await _mediator.Send(new GetCohortSummaryQuery(message.CohortId));

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
            {"employer_hashed_account", _encodingService.Encode(cohortSummary.AccountId, EncodingType.AccountId) },
            {"cohort_reference", _encodingService.Encode(cohortSummary.CohortId, EncodingType.CohortReference)}
        };

        var templateName = cohortSummary.LevyStatus == ApprenticeshipEmployerType.Levy
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
            { "RequestUrl", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{cohortSummary.ProviderId}/unapproved/{cohortSummary.CohortReference}/details" }
        };

        await context.Send(new SendEmailToProviderCommand(cohortSummary.ProviderId.Value, TemplateProviderApprenticeshipChangeOfProviderRequested, tokens));

        _logger.LogInformation($"Sent SendEmailToProviderCommand with template: ProviderApprenticeshipChangeOfProviderRequested");
    }

    private async Task<string> GetApprenticeNamePossessive(long apprenticeshipId)
    {
        var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, default);

        return apprenticeship.ApprenticeName.EndsWith('s') ? apprenticeship.ApprenticeName + "'" : apprenticeship.ApprenticeName + "'s";
    }
}