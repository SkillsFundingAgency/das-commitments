using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingApprenticeChanges
{
    public class GetPendingApprenticeChangesQueryHandler : IRequestHandler<GetPendingApprenticeChangesQuery, GetApprenticeshipUpdateQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        public GetPendingApprenticeChangesQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetApprenticeshipUpdateQueryResult> Handle(GetPendingApprenticeChangesQuery request, CancellationToken cancellationToken)
        {
            var db = _db.Value;

            var apprenticeshipUpdates = db.ApprenticeshipUpdates
             .Where(u => u.Apprenticeship.Cohort.EmployerAccountId == request.EmployerAccountId)
             .Where(u => u.Status == ApprenticeshipUpdateStatus.Pending
                          && u.Originator == Originator.Provider
                          );

            return new GetApprenticeshipUpdateQueryResult
            {
                ApprenticeshipUpdates = await apprenticeshipUpdates
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
                        StartDate = update.StartDate,
                        ActualStartDate = update.ActualStartDate,
                        EndDate = update.EndDate,
                        DateOfBirth = update.DateOfBirth,
                        CreatedOn = update.CreatedOn
                    }).ToListAsync(cancellationToken)
            };
        }
    }
}
