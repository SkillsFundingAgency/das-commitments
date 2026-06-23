using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;

public class UpdateAccountLevyStatusCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateAccountLevyStatusCommandHandler> logger)
    : IRequestHandler<UpdateAccountLevyStatusCommand>
{
    public async Task Handle(UpdateAccountLevyStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Value.Accounts.FirstOrDefaultAsync(x => x.Id == request.AccountId, cancellationToken);

        if (entity != null)
        {
            entity.UpdateLevyStatus(request.LevyStatus);
            await db.Value.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "LevyStatus set to {LevyStatus} for AccountId : {AccountId}",
                request.LevyStatus,
                request.AccountId);
        }
    }
}
