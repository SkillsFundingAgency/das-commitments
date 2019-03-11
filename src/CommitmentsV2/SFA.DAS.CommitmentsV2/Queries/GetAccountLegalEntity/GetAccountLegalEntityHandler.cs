using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Queries.GetAccountLegalEntity
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
                .AccountLegalEntities
                .Include( ale => ale.Account)
                .Where(ale => ale.Id == request.AccountLegalEntityId)
                .AsNoTracking()
                .Select(ale => new GetAccountLegalEntityResponse {AccountName = ale.Account.Name, LegalEntityName = ale.Name})
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
