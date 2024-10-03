using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

public class GetAccountLegalEntityQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityQueryResult>
{
    public Task<GetAccountLegalEntityQueryResult> Handle(GetAccountLegalEntityQuery query, CancellationToken cancellationToken)
    {
        return dbContext.Value
            .AccountLegalEntities.GetById(
                query.AccountLegalEntityId,
                ale => new GetAccountLegalEntityQueryResult
                {
                    AccountId = ale.AccountId,
                    MaLegalEntityId = ale.MaLegalEntityId,
                    AccountName = ale.Account.Name,
                    LegalEntityName = ale.Name,
                    LevyStatus = ale.Account.LevyStatus
                },
                cancellationToken);
    }
}