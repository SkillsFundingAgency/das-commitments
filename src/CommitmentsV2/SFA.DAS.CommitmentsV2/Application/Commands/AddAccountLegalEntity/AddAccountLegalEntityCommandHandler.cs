using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.ProviderRelationships.Application.Commands.AddAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity
{
    public class AddAccountLegalEntityCommandHandler : AsyncRequestHandler<AddAccountLegalEntityCommand>
    {
        private readonly Lazy<AccountsDbContext> _db;

        public AddAccountLegalEntityCommandHandler(Lazy<AccountsDbContext> db)
        {
            _db = db;
        }

        protected override async Task Handle(AddAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
            
            account.AddAccountLegalEntity(request.AccountLegalEntityId, request.AccountLegalEntityPublicHashedId, request.OrganisationName, request.Created);
        }
    }
}