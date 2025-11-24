using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOverlap;

public class ValidateEmailOverlapQueryHandler(IOverlapCheckService overlapCheckService) : IRequestHandler<ValidateEmailOverlapQuery, ValidateEmailOverlapQueryResult>
{
    public async Task<ValidateEmailOverlapQueryResult> Handle(ValidateEmailOverlapQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
        var endDate = DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

        var apprenticeshipWithOverlap = await overlapCheckService.CheckForEmailOverlaps(request.Email, new Domain.Entities.DateRange(startDate, endDate), request.DraftApprenticeshipId, request.CohortId, cancellationToken);

        var result = new ValidateEmailOverlapQueryResult { OverlapStatus =  apprenticeshipWithOverlap.OverlapStatus };

        return await Task.FromResult(result);
    }
}
