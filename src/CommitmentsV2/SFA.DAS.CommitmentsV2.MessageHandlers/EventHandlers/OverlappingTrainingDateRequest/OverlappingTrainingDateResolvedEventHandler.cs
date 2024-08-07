﻿using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;

public class OverlappingTrainingDateResolvedEventHandler : IHandleMessages<OverlappingTrainingDateResolvedEvent>
{
    private readonly ILogger<OverlappingTrainingDateResolvedEventHandler> _logger;
    private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    public OverlappingTrainingDateResolvedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
        ILogger<OverlappingTrainingDateResolvedEventHandler> logger,
        CommitmentsV2Configuration commitmentsV2Configuration)
    {
        _logger = logger;
        _commitmentsV2Configuration = commitmentsV2Configuration;
        _dbContext = dbContext;
    }

    public async Task Handle(OverlappingTrainingDateResolvedEvent message, IMessageHandlerContext context)
    {
        try
        {
            _logger.LogInformation($"Received {nameof(OverlappingTrainingDateResolvedEvent)} for DraftApprenticeship {message?.ApprenticeshipId}");

            if (message != null)
            {
                var draftApprenticeship = await _dbContext.Value.GetOLTDResolvedDraftApprenticeshipAggregate(message.CohortId, message.ApprenticeshipId, default);

                var sendEmailToProviderCommand = BuildEmailToEmployerCommand(draftApprenticeship);

                await context.Send(sendEmailToProviderCommand, new SendOptions());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Send message to provider for DraftApprenticeship {message?.ApprenticeshipId}");
            throw;
        }
    }

    private SendEmailToProviderCommand BuildEmailToEmployerCommand(DraftApprenticeship draftApprenticeship)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(draftApprenticeship.Cohort.ProviderId,
            "ProviderOverlappingTrainingDateClosed",
            new Dictionary<string, string>
            {
                {"CohortReference",draftApprenticeship.Cohort.Reference},
                {"Url", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{draftApprenticeship.Cohort.ProviderId}/unapproved/{draftApprenticeship.Cohort.Reference}/details"}
            }, draftApprenticeship.Cohort.LastUpdatedByProviderEmail
        );

        return sendEmailToProviderCommand;
    }
}