using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

public class GetAccountSummaryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>
{
    public async Task<GetAccountSummaryQueryResult> Handle(GetAccountSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Value.Accounts
            .SingleAsync(a => a.Id == query.AccountId, cancellationToken: cancellationToken);

        return new GetAccountSummaryQueryResult
        {
            AccountId = query.AccountId,
            LevyStatus = account.LevyStatus
        };
    }
}