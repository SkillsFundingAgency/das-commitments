using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity
{
    public class GetAccountLegalEntityQueryHandler : IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountLegalEntityQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetAccountLegalEntityQueryResult> Handle(GetAccountLegalEntityQuery query, CancellationToken cancellationToken)
        {
            return _dbContext.Value
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
}
