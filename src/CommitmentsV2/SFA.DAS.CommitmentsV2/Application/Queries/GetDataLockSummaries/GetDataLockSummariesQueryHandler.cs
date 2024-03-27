using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;

public class GetDataLockSummariesQueryHandler : IRequestHandler<GetDataLockSummariesQuery, GetDataLockSummariesQueryResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    public GetDataLockSummariesQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) => _dbContext = dbContext;

    public async Task<GetDataLockSummariesQueryResult> Handle(GetDataLockSummariesQuery request, CancellationToken cancellationToken)
    {
        var dataLocks = await _dbContext.Value.DataLocks
            .Where(x => x.ApprenticeshipId == request.ApprenticeshipId && x.EventStatus != EventStatus.Removed && !x.IsExpired)
            .ToListAsync(cancellationToken: cancellationToken);    

        return new GetDataLockSummariesQueryResult
        {
            DataLocksWithCourseMismatch = dataLocks
                .Where(DataLockStatusExtensions.UnHandled)
                .Where(DataLockStatusExtensions.WithCourseError)
                .Select(source => new DataLock
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
                }).ToList(),

            DataLocksWithOnlyPriceMismatch = dataLocks
                .Where(DataLockStatusExtensions.UnHandled)
                .Where(DataLockStatusExtensions.IsPriceOnly)
                .Select(source => new DataLock
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
                }).ToList()
        };
    }
}