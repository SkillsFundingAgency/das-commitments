using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.FindLearner
{
    public class FindLearnerQueryHandler : IRequestHandler<FindLearnerQuery, FindLearnerQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public FindLearnerQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FindLearnerQueryResult> Handle(FindLearnerQuery request, CancellationToken cancellationToken)
        {
            var firstNameParam = new SqlParameter("firstName", request.FirstName);
            var lastNameParam = new SqlParameter("lastName", request.LastName);
            var dateOfBirthParam = new SqlParameter("dateOfBirth", request.DateOfBirth);

            var dbLearners = await _dbContext.Value.Learners
                                .FromSqlRaw("exec FindLearner @firstName, @lastName, @dateOfBirth", firstNameParam, lastNameParam, dateOfBirthParam)
                                .ToListAsync();

            return new FindLearnerQueryResult(dbLearners.Select(l => (Learner)l).ToList());
        }
    }
}
