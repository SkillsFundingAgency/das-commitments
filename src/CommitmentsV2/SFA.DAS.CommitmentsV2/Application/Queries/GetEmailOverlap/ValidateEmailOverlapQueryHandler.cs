using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Globalization;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOverlap;

public class ValidateEmailOverlapQueryHandler(IOverlapCheckService overlapCheckService) : IRequestHandler<ValidateEmailOverlapQuery, ValidateEmailOverlapQueryResult>
{
    public async Task<ValidateEmailOverlapQueryResult> Handle(ValidateEmailOverlapQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.Parse(request.StartDate).Date;
        var endDate = DateTime.Parse(request.EndDate).Date;

        var apprenticeshipWithOverlap = await overlapCheckService.CheckForEmailOverlaps(request.Email, new Domain.Entities.DateRange(startDate, endDate), request.DraftApprenticeshipId, request.CohortId, cancellationToken);

        var result = apprenticeshipWithOverlap is not null ? new ValidateEmailOverlapQueryResult { OverlapStatus = apprenticeshipWithOverlap.OverlapStatus } : new ValidateEmailOverlapQueryResult();

        return await Task.FromResult(result);
    }
}
