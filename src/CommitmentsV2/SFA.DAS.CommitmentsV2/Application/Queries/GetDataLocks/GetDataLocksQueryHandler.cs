﻿using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks
{
    public class GetDataLocksQueryHandler : IRequestHandler<GetDataLocksQuery, GetDataLocksQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDataLocksQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<GetDataLocksQueryResult> Handle(GetDataLocksQuery request, CancellationToken cancellationToken)
        {
            var dataLocks = _dbContext.Value.DataLocks.Where(x => x.ApprenticeshipId == request.ApprenticeshipId
                          && x.EventStatus != Types.EventStatus.Removed && !x.IsExpired);

            return new GetDataLocksQueryResult
            {
                DataLocks = await dataLocks.Select(source => new GetDataLocksQueryResult.DataLock
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
}
