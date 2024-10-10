using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;

public class ValidateUlnOverlapOnStartDateQueryHandler(IOverlapCheckService overlapCheckService) : IRequestHandler<ValidateUlnOverlapOnStartDateQuery, ValidateUlnOverlapOnStartDateQueryResult>
{
    public async Task<ValidateUlnOverlapOnStartDateQueryResult> Handle(ValidateUlnOverlapOnStartDateQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
        var endDate = DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

        var apprenticeshipWithOverlap = await overlapCheckService.CheckForOverlapsOnStartDate(request.Uln, new Domain.Entities.DateRange(startDate, endDate), null, cancellationToken);

        var result = new ValidateUlnOverlapOnStartDateQueryResult { HasStartDateOverlap = apprenticeshipWithOverlap.HasOverlappingStartDate, HasOverlapWithApprenticeshipId = apprenticeshipWithOverlap.ApprenticeshipId };

        return await Task.FromResult(result);
    }
}