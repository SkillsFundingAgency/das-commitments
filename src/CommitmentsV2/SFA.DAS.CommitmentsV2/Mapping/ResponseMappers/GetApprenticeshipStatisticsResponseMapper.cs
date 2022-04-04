using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipStatisticsResponseMapper : IMapper<GetApprenticeshipStatisticsQueryResult, GetApprenticeshipStatisticsResponse>
    {
        public Task<GetApprenticeshipStatisticsResponse> Map(GetApprenticeshipStatisticsQueryResult source)
        {
            return Task.FromResult(new GetApprenticeshipStatisticsResponse
            {
                PausedApprenticeshipCount = source.PausedApprenticeshipCount,
                StoppedApprenticeshipCount = source.StoppedApprenticeshipCount,
                ApprovedApprenticeshipCount = source.ApprovedApprenticeshipCount
            });
        }
    }
}
