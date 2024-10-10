using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;

public class GetDataLocksQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetDataLocksQuery, GetDataLocksQueryResult>
{
    public async Task<GetDataLocksQueryResult> Handle(GetDataLocksQuery request, CancellationToken cancellationToken)
    {
        var dataLocks = dbContext.Value.DataLocks.Where(x => x.ApprenticeshipId == request.ApprenticeshipId
                                                              && x.EventStatus != Types.EventStatus.Removed && !x.IsExpired);

        return new GetDataLocksQueryResult
        {
            DataLocks = await dataLocks.Select(source => new DataLock
            {
                Id = source.Id,
                DataLockEventDatetime = source.DataLockEventDatetime,
                PriceEpisodeIdentifier = source.PriceEpisodeIdentifier,
                ApprenticeshipId = source.ApprenticeshipId,
                IlrTrainingCourseCode = source.IlrTrainingCourseCode,
                IlrActualStartDate = source.IlrActualStartDate,
                IlrEffectiveFromDate = source.IlrEffectiveFromDate,
                IlrPriceEffectiveToDate = source.IlrPriceEffectiveToDate,
                IlrTotalCost = source.IlrTotalCost,
                ErrorCode = source.ErrorCode,
                DataLockStatus = source.Status,
                TriageStatus = source.TriageStatus,
                IsResolved = source.IsResolved
            }).ToListAsync(cancellationToken)
        };
    }
}