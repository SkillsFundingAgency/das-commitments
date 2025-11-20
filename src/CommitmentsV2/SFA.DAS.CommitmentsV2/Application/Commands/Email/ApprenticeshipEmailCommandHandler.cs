using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class ApprenticeshipEmailCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprenticeshipEmailCommandHandler> logger)
    : IRequestHandler<ApprenticeshipEmailCommand>
{
    public async Task Handle(ApprenticeshipEmailCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);


        apprenticeship.SetEmail(command.Email);

        logger.LogInformation("Set Email  for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);
    }
}