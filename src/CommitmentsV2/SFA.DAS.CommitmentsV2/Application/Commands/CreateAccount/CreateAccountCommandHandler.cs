using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateAccount;

public class CreateAccountCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<CreateAccountCommandHandler> logger)
    : IRequestHandler<CreateAccountCommand>
{
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{TypeName} processing started. Persisting account for request: {Request}.", nameof(CreateAccountCommandHandler), JsonSerializer.Serialize(request));

        var dbContext = db.Value;
        var existing = dbContext.Accounts.FirstOrDefault(a => a.Id == request.AccountId);

        if (existing != null)
        {
            logger.LogWarning("Account with Id {AccountId} already exists. No action needed.", request.AccountId);
            return;
        }

        var account = new Account(request.AccountId, request.HashedId, request.PublicHashedId, request.Name, request.Created);

        await dbContext.Accounts.AddAsync(account, cancellationToken);
            
        logger.LogInformation("{TypeName} processing completed.", nameof(CreateAccountCommandHandler));
    }
}