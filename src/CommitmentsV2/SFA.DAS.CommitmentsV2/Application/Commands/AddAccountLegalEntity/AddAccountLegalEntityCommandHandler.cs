using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity
{
    public class AddAccountLegalEntityCommandHandler : IRequestHandler<AddAccountLegalEntityCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public AddAccountLegalEntityCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task Handle(AddAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
            
            account.AddAccountLegalEntity(request.AccountLegalEntityId, request.MaLegalEntityId, request.OrganisationReferenceNumber, 
                request.AccountLegalEntityPublicHashedId, request.OrganisationName, request.OrganisationType, 
                request.OrganisationAddress, request.Created);
        }
    }
}