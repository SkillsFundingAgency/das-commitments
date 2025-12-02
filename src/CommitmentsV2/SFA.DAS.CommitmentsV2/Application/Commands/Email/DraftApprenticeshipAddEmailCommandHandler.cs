using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class DraftApprenticeshipAddEmailCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipAddEmailCommandHandler> logger)
    : IRequestHandler<DraftApprenticeshipAddEmailCommand>
{
    public async Task Handle(DraftApprenticeshipAddEmailCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        apprenticeship?.SetEmail(command.Email);

        logger.LogInformation("Set Email  for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);
    }
}