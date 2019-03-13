using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity
{
    public class RemoveAccountLegalEntityCommandHandler : AsyncRequestHandler<RemoveAccountLegalEntityCommand>
    {
        private readonly Lazy<AccountsDbContext> _db;

        public RemoveAccountLegalEntityCommandHandler(Lazy<AccountsDbContext> db)
        {
            _db = db;
        }

        protected override async Task Handle(RemoveAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
            
            var accountLegalEntity = await _db.Value.AccountLegalEntities
                .IgnoreQueryFilters()
                .SingleAsync(ale => ale.Id == request.AccountLegalEntityId, cancellationToken);
            
            account.RemoveAccountLegalEntity(accountLegalEntity, request.Removed);
        }
    }
}