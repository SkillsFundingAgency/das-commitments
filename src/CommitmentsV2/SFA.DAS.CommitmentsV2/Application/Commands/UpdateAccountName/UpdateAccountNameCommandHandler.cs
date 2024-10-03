using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountName;

public class UpdateAccountNameCommandHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<UpdateAccountNameCommand>
{
    public async Task Handle(UpdateAccountNameCommand request, CancellationToken cancellationToken)
    {
        var account = await db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);

        account.UpdateName(request.Name, request.Created);
    }
}