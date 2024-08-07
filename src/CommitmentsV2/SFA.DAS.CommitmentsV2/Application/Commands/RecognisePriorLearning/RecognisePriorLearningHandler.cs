﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;

public class RecognisePriorLearningHandler : IRequestHandler<RecognisePriorLearningCommand>
{
    private readonly ILogger<RecognisePriorLearningHandler> _logger;
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    public RecognisePriorLearningHandler(
        Lazy<ProviderCommitmentsDbContext> dbContext,
        ILogger<RecognisePriorLearningHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(RecognisePriorLearningCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await _dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        apprenticeship.SetRecognisePriorLearning(command.RecognisePriorLearning);

        _logger.LogInformation("Set RecognisePriorLearning to {RecognisePriorLearning} for draft Apprenticeship:{ApprenticeshipId}", command.RecognisePriorLearning, command.ApprenticeshipId);
    }
}