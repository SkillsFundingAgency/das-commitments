using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

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
            if (null == sinceTimeParam.Value)
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
                                .FromSqlRaw("exec GetLearnersBatch @sinceTime, @batchNumber OUTPUT, @batchSize OUTPUT, @totalNumberOfBatches OUTPUT", sinceTimeParam, batchNumberParam, batchSizeParam, totalNumberOfBatchesParam)
                                .ToList();

            var learners = dblearners.Select(l => (Learner)l).ToList();

            int totalNumberOfBatches = (DBNull.Value == totalNumberOfBatchesParam.Value) ? 0 : (int)totalNumberOfBatchesParam.Value;
            return Task.FromResult(new GetAllLearnersQueryResult(learners, (int)batchNumberParam.Value, (int)batchSizeParam.Value, totalNumberOfBatches));
        }
    }
}
