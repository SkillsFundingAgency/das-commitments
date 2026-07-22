using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprenticeshipApprovalQuery, GetApprenticeshipApprovalQueryResult>
{
    public async Task<GetApprenticeshipApprovalQueryResult> Handle(GetApprenticeshipApprovalQuery request, CancellationToken cancellationToken)
    {
        var approvalRequest = await dbContext.Value.ApprovalRequests
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.ApprovalRequestId, cancellationToken);

        if (approvalRequest == null)
        {
            return null;
        }

        if (approvalRequest.ApprenticeshipId != request.ApprenticeshipId)
        {
            throw new BadRequestException($"ApprenticeshipId {request.ApprenticeshipId} does not match on Approval Request record");
        }

        var apprenticeship = await dbContext.Value.Apprenticeships
            .Include(x => x.Cohort).ThenInclude(x => x.Provider)
            .Include(a => a.Cohort).ThenInclude(c => c.AccountLegalEntity)
            .FirstOrDefaultAsync(x => x.Id == request.ApprenticeshipId, cancellationToken);


        return new GetApprenticeshipApprovalQueryResult
        {
            ApprenticeshipId = apprenticeship.Id,
            Name = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
            Uln = apprenticeship.Uln,
            ProviderId = apprenticeship.Cohort.Provider.Id,
            ProviderName = apprenticeship.Cohort.Provider.Name,
            AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntity.Id,
            AccountLegalEntityName = apprenticeship.Cohort.AccountLegalEntity.Name,
            TrainingCode = apprenticeship.TrainingCode,
            TrainingName = apprenticeship.TrainingName,
            Cost = apprenticeship.Cost,
            StartDate = apprenticeship.StartDate,
            EndDate = apprenticeship.EndDate,
            DeliveryModel = (DeliveryModel)apprenticeship.DeliveryModel,
            OriginatorReference = apprenticeship.OriginatorReference,
            ReservationId = apprenticeship.ReservationId,
            EmployerReference = apprenticeship.EmployerReference
        };
    }
}


