using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity
{
    public class AddAccountLegalEntityCommandHandler : IRequestHandler<AddAccountLegalEntityCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<AddAccountLegalEntityCommandHandler> _logger;

        public AddAccountLegalEntityCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<AddAccountLegalEntityCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(AddAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{TypeName} processing started.", nameof(AddAccountLegalEntityCommandHandler));
            
            _logger.LogInformation("Retrieving account with Id: {AccountId}.", request.AccountId);
            
            var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
            
            account.AddAccountLegalEntity(request.AccountLegalEntityId, request.MaLegalEntityId, request.OrganisationReferenceNumber, 
                request.AccountLegalEntityPublicHashedId, request.OrganisationName, request.OrganisationType, 
                request.OrganisationAddress, request.Created);
            
            _logger.LogInformation("{TypeName} processing completed.", nameof(AddAccountLegalEntityCommandHandler));
        }
    }
}