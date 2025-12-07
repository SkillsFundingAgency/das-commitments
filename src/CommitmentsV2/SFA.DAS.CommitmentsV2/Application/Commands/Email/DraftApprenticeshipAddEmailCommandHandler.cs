using Azure;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class DraftApprenticeshipAddEmailCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<DraftApprenticeshipAddEmailCommandHandler> logger,
    IViewEditDraftApprenticeshipEmailValidationService viewEditDraftApprenticeshipEmailValidationService)
    : IRequestHandler<DraftApprenticeshipAddEmailCommand, DraftApprenticeshipAddEmailResult>
{
    public async Task<DraftApprenticeshipAddEmailResult> Handle(DraftApprenticeshipAddEmailCommand command, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.GetDraftApprenticeshipAggregate(command.CohortId, command.ApprenticeshipId, cancellationToken);

        var response = await viewEditDraftApprenticeshipEmailValidationService.Validate( 
            
            new ViewEditDraftApprenticeshipEmailValidationRequest()
            {
                DraftApprenticeshipId = command.ApprenticeshipId,
                CohortId = command.CohortId,
                Email = command.Email,
                EndDate = command.EndDate,
                StartDate = command.StartDate,

            }, cancellationToken);

        response?.Errors?.ThrowIfAny();

        apprenticeship?.SetEmail(command.Email);

        logger.LogInformation("Set Email  for draft Apprenticeship:{ApprenticeshipId}", command.ApprenticeshipId);

        return new DraftApprenticeshipAddEmailResult
        {
            DraftApprenticeshipId = command.ApprenticeshipId
        };
    }
}