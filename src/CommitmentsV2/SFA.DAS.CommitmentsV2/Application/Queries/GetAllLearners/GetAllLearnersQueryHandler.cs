using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners
{
    public class GetAllLearnersQueryHandler : IRequestHandler<GetAllLearnersQuery, GetAllLearnersQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAllLearnersQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetAllLearnersQueryResult> Handle(GetAllLearnersQuery request, CancellationToken cancellationToken)
        {
            var sinceTimeParam = new SqlParameter("sinceTime", request.SinceTime);
            if(null == sinceTimeParam.Value)
            {
                sinceTimeParam.Value = DBNull.Value;
            }
            var batchNumberParam = new SqlParameter("batchNumber", request.BatchNumber);
            batchNumberParam.Direction = System.Data.ParameterDirection.InputOutput;
            var batchSizeParam = new SqlParameter("batchSize", request.BatchSize);
            batchSizeParam.Direction = System.Data.ParameterDirection.InputOutput;
            var totalNumberOfBatchesParam = new SqlParameter("totalNumberOfBatches", System.Data.SqlDbType.Int);
            totalNumberOfBatchesParam.Direction = System.Data.ParameterDirection.Output;

            var dblearners = _dbContext.Value.Learners
                                .FromSql("exec GetLearnersBatch @sinceTime, @batchNumber OUTPUT, @batchSize OUTPUT, @totalNumberOfBatches OUTPUT", sinceTimeParam, batchNumberParam, batchSizeParam, totalNumberOfBatchesParam)
                                .ToList();

            // @ToDo: can we use AutoMapper?
            var learners = new List<Learner>();
            foreach (var dblearner in dblearners)
            {
                learners.Add(new Learner()
                {
                    ApprenticeshipId = dblearner.ApprenticeshipId,
                    FirstName = dblearner.FirstName,
                    LastName = dblearner.LastName,
                    ULN = dblearner.ULN,
                    TrainingCode = dblearner.TrainingCode,
                    TrainingCourseVersion = dblearner.TrainingCourseVersion,
                    TrainingCourseVersionConfirmed = dblearner.TrainingCourseVersionConfirmed,
                    TrainingCourseOption = dblearner.TrainingCourseOption,
                    StandardUId = dblearner.StandardUId,
                    StartDate = dblearner.StartDate,
                    EndDate = dblearner.EndDate,
                    CreatedOn = dblearner.CreatedOn,
                    UpdatedOn = dblearner.UpdatedOn,
                    StopDate = dblearner.StopDate,
                    PauseDate = dblearner.PauseDate,
                    CompletionDate = dblearner.CompletionDate,
                    UKPRN = dblearner.UKPRN,
                    LearnRefNumber = dblearner.LearnRefNumber,
                    PaymentStatus = dblearner.PaymentStatus
                });
            }

            int totalNumberOfBatches = (DBNull.Value == totalNumberOfBatchesParam.Value) ? 0 : (int)totalNumberOfBatchesParam.Value;
            return Task.FromResult(new GetAllLearnersQueryResult(learners, (int)batchNumberParam.Value, (int)batchSizeParam.Value, totalNumberOfBatches));
        }
    }
}
