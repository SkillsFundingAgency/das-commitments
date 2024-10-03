using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy;

public class UpdateLevyStatusToLevyCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateLevyStatusToLevyCommandHandler> logger)
    : IRequestHandler<UpdateLevyStatusToLevyCommand>
{
    public async Task Handle(UpdateLevyStatusToLevyCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Value.Accounts.FirstOrDefaultAsync(x => x.Id == request.AccountId, cancellationToken);

        if (entity != null)
        {
            entity.UpdateLevyStatus(ApprenticeshipEmployerType.Levy);
            await db.Value.SaveChangesAsync(cancellationToken);
            logger.LogInformation("LevyStatus set to Levy for AccountId : {AccountId}", request.AccountId);
        }
    }
}