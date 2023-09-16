using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<CreateAccountCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{TypeName} processing started.", nameof(CreateAccountCommandHandler));
            
            _logger.LogInformation("Persisting account for request: {Request}.", request);
            
            var account = new Account(request.AccountId, request.HashedId, request.PublicHashedId, request.Name, request.Created);

            await _db.Value.Accounts.AddAsync(account, cancellationToken);
            
            _logger.LogInformation("{TypeName} processing completed.", nameof(CreateAccountCommandHandler));
        }
    }
}