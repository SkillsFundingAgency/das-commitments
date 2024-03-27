using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;

public class GetAllLearnersQueryHandler : IRequestHandler<GetAllLearnersQuery, GetAllLearnersQueryResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    public GetAllLearnersQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) => _dbContext = dbContext;

    public Task<GetAllLearnersQueryResult> Handle(GetAllLearnersQuery request, CancellationToken cancellationToken)
    {
        var sinceTimeParam = new SqlParameter("sinceTime", request.SinceTime);

        if (null == sinceTimeParam.Value)
        {
            sinceTimeParam.Value = DBNull.Value;
        }

        var batchNumberParam = new SqlParameter("batchNumber", request.BatchNumber) { Direction = System.Data.ParameterDirection.InputOutput };
        var batchSizeParam = new SqlParameter("batchSize", request.BatchSize) { Direction = System.Data.ParameterDirection.InputOutput };
        var totalNumberOfBatchesParam = new SqlParameter("totalNumberOfBatches", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };

        var learnersBatch = _dbContext.Value.Learners
            .FromSqlRaw("exec GetLearnersBatch @sinceTime, @batchNumber OUTPUT, @batchSize OUTPUT, @totalNumberOfBatches OUTPUT", sinceTimeParam, batchNumberParam, batchSizeParam, totalNumberOfBatchesParam)
            .ToList();

        var learners = learnersBatch.Select(learnerItem => new Learner
            {
                ApprenticeshipId = learnerItem.ApprenticeshipId,
                FirstName = learnerItem.FirstName,
                LastName = learnerItem.LastName,
                ULN = learnerItem.ULN,
                TrainingCode = learnerItem.TrainingCode,
                TrainingCourseVersion = learnerItem.TrainingCourseVersion,
                TrainingCourseVersionConfirmed = learnerItem.TrainingCourseVersionConfirmed,
                TrainingCourseOption = learnerItem.TrainingCourseOption,
                StandardUId = learnerItem.StandardUId,
                StartDate = learnerItem.StartDate,
                EndDate = learnerItem.EndDate,
                CreatedOn = learnerItem.CreatedOn,
                UpdatedOn = learnerItem.UpdatedOn,
                StopDate = learnerItem.StopDate,
                PauseDate = learnerItem.PauseDate,
                CompletionDate = learnerItem.CompletionDate,
                UKPRN = learnerItem.UKPRN,
                LearnRefNumber = learnerItem.LearnRefNumber,
                PaymentStatus = learnerItem.PaymentStatus,
                EmployerAccountId = learnerItem.EmployerAccountId,
                EmployerName = learnerItem.EmployerName,
            })
            .ToList();

        var totalNumberOfBatches = DBNull.Value == totalNumberOfBatchesParam.Value ? 0 : (int)totalNumberOfBatchesParam.Value;

        var result = new GetAllLearnersQueryResult(learners, (int)batchNumberParam.Value, (int)batchSizeParam.Value, totalNumberOfBatches);

        return Task.FromResult(result);
    }
}