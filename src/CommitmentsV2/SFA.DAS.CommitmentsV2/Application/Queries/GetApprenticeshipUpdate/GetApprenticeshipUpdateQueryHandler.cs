using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate
{
    public class GetApprenticeshipUpdateQueryHandler : IRequestHandler<GetApprenticeshipUpdateQuery, GetApprenticeshipUpdateQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetApprenticeshipUpdateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprenticeshipUpdateQueryResult> Handle(GetApprenticeshipUpdateQuery request, CancellationToken cancellationToken)
        {
            GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate apprenticeshipUpdate = null;
            var update = await _dbContext.Value.ApprenticeshipUpdates.FirstOrDefaultAsync(
                    x => x.ApprenticeshipId == request.ApprenticeshipId && x.Status == ApprenticeshipUpdateStatus.Pending, cancellationToken);

            if (update != null)
            {
                apprenticeshipUpdate = new GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate
                {
                    Id = update.Id,
                    ApprenticeshipId = update.ApprenticeshipId,
                    Originator = update.Originator,
                    FirstName = update.FirstName,
                    LastName = update.LastName,
                    TrainingType = update.TrainingType,
                    TrainingCode = update.TrainingCode,
                    TrainingName = update.TrainingName,
                    Cost = update.Cost,
                    StartDate = update.StartDate,
                    EndDate = update.EndDate,
                    DateOfBirth = update.DateOfBirth
                };
            }

            return new GetApprenticeshipUpdateQueryResult
            {
                PendingApprenticeshipUpdate = apprenticeshipUpdate
            };
        }
    }
}
