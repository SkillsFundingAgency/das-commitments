using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryHandler : IRequestHandler<GetAccountSummaryRequest, GetAccountSummaryResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountSummaryResponse> Handle(GetAccountSummaryRequest request, CancellationToken cancellationToken)
        {
            //todo: what if account doesn't exist? do we pay the cost of account.GetById in order to do a 404 if not?

            var result = await (_dbContext.Value
                .Cohorts
                .Where(c => c.EmployerAccountId == request.AccountId)
                .Select(c =>
                    new
                    {
                        IsApproved = c.EditStatus == EditStatus.Both &&
                                     (!c.TransferSenderId.HasValue ||
                                      c.TransferApprovalStatus == TransferApprovalStatus.Approved)
                    })).ToListAsync(cancellationToken);

            return new GetAccountSummaryResponse
            {
                AccountId = request.AccountId,
                HasCohorts = result.Any(x => !x.IsApproved),
                HasApprenticeships = result.Any(x => x.IsApproved)
            };

        }
    }
}
