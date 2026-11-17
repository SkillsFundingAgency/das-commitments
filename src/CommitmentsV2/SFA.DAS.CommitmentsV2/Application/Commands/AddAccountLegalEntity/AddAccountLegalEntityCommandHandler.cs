using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddAccountLegalEntity;

public class AddAccountLegalEntityCommandHandler(
    Lazy<ProviderCommitmentsDbContext> db,
    ILogger<AddAccountLegalEntityCommandHandler> logger)
    : IRequestHandler<AddAccountLegalEntityCommand>
{
    public async Task Handle(AddAccountLegalEntityCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{TypeName} processing started. Retrieving account with Id: {AccountId}.", nameof(AddAccountLegalEntityCommandHandler), request.AccountId);

        var alreadyExists = await db.Value.AccountLegalEntities
            .IgnoreQueryFilters()
            .AnyAsync(ale => ale.Id == request.AccountLegalEntityId, cancellationToken);

        if (alreadyExists)
        {
            logger.LogInformation("{TypeName} skipped. Account legal entity {AccountLegalEntityId} already exists.", nameof(AddAccountLegalEntityCommandHandler), request.AccountLegalEntityId);
            return;
        }

        var account = await db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);

        account.AddAccountLegalEntity(
            request.AccountLegalEntityId,
            request.MaLegalEntityId,
            request.OrganisationReferenceNumber,
            request.AccountLegalEntityPublicHashedId,
            request.OrganisationName,
            request.OrganisationType,
            request.OrganisationAddress,
            request.Created
        );

        logger.LogInformation("{TypeName} processing completed.", nameof(AddAccountLegalEntityCommandHandler));
    }
}