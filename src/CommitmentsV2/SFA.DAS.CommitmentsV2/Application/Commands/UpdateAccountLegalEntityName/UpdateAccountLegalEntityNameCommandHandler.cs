using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLegalEntityName;

public class UpdateAccountLegalEntityNameCommandHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<UpdateAccountLegalEntityNameCommand>
{
    public async Task Handle(UpdateAccountLegalEntityNameCommand request, CancellationToken cancellationToken)
    {
        var accountLegalEntity = await db.Value.AccountLegalEntities.IgnoreQueryFilters().SingleAsync(a => a.Id == request.AccountLegalEntityId, cancellationToken);

        accountLegalEntity.UpdateName(request.Name, request.Created);
    }
}