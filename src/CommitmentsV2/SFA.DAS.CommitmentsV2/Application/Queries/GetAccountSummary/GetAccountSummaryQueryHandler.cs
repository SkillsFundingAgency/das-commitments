using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryQueryHandler : IRequestHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountSummaryQueryResult> Handle(GetAccountSummaryQuery query,
            CancellationToken cancellationToken)
        {
            var account = await _dbContext.Value.Accounts
               .SingleAsync(a => a.Id == query.AccountId, cancellationToken: cancellationToken);

            return new GetAccountSummaryQueryResult
            {
                AccountId = query.AccountId,
                LevyStatus = account.LevyStatus
            };
        }
    }
}
