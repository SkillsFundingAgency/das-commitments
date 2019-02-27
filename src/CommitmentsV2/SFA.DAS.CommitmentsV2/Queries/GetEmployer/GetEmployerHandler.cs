using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Queries.GetEmployer
{
    public class GetEmployerHandler : IRequestHandler<GetEmployerRequest, GetEmployerResponse>
    {
        private readonly Lazy<AccountsDbContext> _dbContext;

        public GetEmployerHandler(Lazy<AccountsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetEmployerResponse> Handle(GetEmployerRequest request, CancellationToken cancellationToken)
        {
            return _dbContext.Value
                .AccountLegalEntities
                .Include( ale => ale.Account)
                .Where(ale => ale.Id == request.AccountLegalEntityId)
                .AsNoTracking()
                .Select(ale => new GetEmployerResponse {AccountName = ale.Account.Name, LegalEntityName = ale.Name})
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
