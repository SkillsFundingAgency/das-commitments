﻿using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;

public class GetApprenticeshipUpdateQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprenticeshipUpdateQuery, GetApprenticeshipUpdateQueryResult>
{
    public async Task<GetApprenticeshipUpdateQueryResult> Handle(GetApprenticeshipUpdateQuery request, CancellationToken cancellationToken)
    {
        var apprenticeships = dbContext.Value.ApprenticeshipUpdates.Where(x => x.ApprenticeshipId == request.ApprenticeshipId);

        if (request.Status.HasValue)
        {
            apprenticeships = apprenticeships.Where(x => x.Status == request.Status);
        }

        return new GetApprenticeshipUpdateQueryResult
        {
            ApprenticeshipUpdates = await apprenticeships
                .Select(update => new GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate
                {
                    Id = update.Id,
                    ApprenticeshipId = update.ApprenticeshipId,
                    Originator = update.Originator,
                    FirstName = update.FirstName,
                    LastName = update.LastName,
                    Email = update.Email,
                    DeliveryModel = update.DeliveryModel,
                    EmploymentEndDate = update.EmploymentEndDate,
                    EmploymentPrice = update.EmploymentPrice,
                    TrainingType = update.TrainingType,
                    TrainingCode = update.TrainingCode,
                    TrainingName = update.TrainingName,
                    TrainingCourseVersion = update.TrainingCourseVersion,
                    TrainingCourseOption = update.TrainingCourseOption,
                    Cost = update.Cost,
                    TrainingPrice = update.TrainingPrice,
                    StartDate = update.StartDate,
                    ActualStartDate = update.ActualStartDate,
                    EndDate = update.EndDate,
                    DateOfBirth = update.DateOfBirth,
                    CreatedOn = update.CreatedOn
                }).ToListAsync(cancellationToken)
        };
    }
}


