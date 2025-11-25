using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class DraftApprenticeshipAddEmailCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipAddEmailCommandHandler> logger,
    IEmailOverlapService emailOverlapService)
    : IRequestHandler<DraftApprenticeshipAddEmailCommand>
{
    public async Task Handle(DraftApprenticeshipAddEmailCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        var validationOfOverlapEmail = await emailOverlapService.GetOverlappingEmails(
            new EmailToValidate(command.Email, apprenticeship.StartDate.GetValueOrDefault(), apprenticeship.EndDate.GetValueOrDefault(), 
            command.ApprenticeshipId),  command.CohortId, cancellationToken);

        if (validationOfOverlapEmail != null)
        {
            apprenticeship.SetEmail(command.Email);
        }

        logger.LogInformation("Set Email  for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);
    }
}