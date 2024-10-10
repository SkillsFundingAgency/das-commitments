using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

public class GetDataLocksResponseMapper : IMapper<GetDataLocksQueryResult, GetDataLocksResponse>
{
    public Task<GetDataLocksResponse> Map(GetDataLocksQueryResult result)
    {
        return Task.FromResult(new GetDataLocksResponse
        {
            DataLocks = result.DataLocks.Select(source => new DataLock
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
                DataLockStatus = source.DataLockStatus,
                TriageStatus = source.TriageStatus,
                IsResolved = source.IsResolved
            }).ToList()
        });
    }
}