using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;

public class CreateAccountCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand>
{
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{TypeName} processing started.", nameof(CreateAccountCommandHandler));
            
        logger.LogInformation("Persisting account for request: {Request}.", request);
            
        var account = new Account(request.AccountId, request.HashedId, request.PublicHashedId, request.Name, request.Created);

        await db.Value.Accounts.AddAsync(account, cancellationToken);
            
        logger.LogInformation("{TypeName} processing completed.", nameof(CreateAccountCommandHandler));
    }
}