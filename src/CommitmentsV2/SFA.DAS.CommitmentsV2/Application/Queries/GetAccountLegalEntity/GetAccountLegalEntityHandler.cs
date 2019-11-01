using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity
{
    public class GetAccountLegalEntityHandler : IRequestHandler<GetAccountLegalEntityRequest, GetAccountLegalEntityResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountLegalEntityHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetAccountLegalEntityResponse> Handle(GetAccountLegalEntityRequest request, CancellationToken cancellationToken)
        {
            return _dbContext.Value
                .AccountLegalEntities.GetById(
                    request.AccountLegalEntityId,
                    ale => new GetAccountLegalEntityResponse
                    {
                        AccountId = ale.AccountId,
                        MaLegalEntityId = ale.MaLegalEntityId,
                        AccountName = ale.Account.Name,
                        LegalEntityName = ale.Name
                    },
                    cancellationToken);
        }
    }
}
