using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateLevyStatusToLevy
{
    public class UpdateLevyStatusToLevyCommandHandler : IRequestHandler<UpdateLevyStatusToLevyCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<UpdateLevyStatusToLevyCommandHandler> _logger;

        public UpdateLevyStatusToLevyCommandHandler(Lazy<ProviderCommitmentsDbContext> db, ILogger<UpdateLevyStatusToLevyCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(UpdateLevyStatusToLevyCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Value.Accounts.FirstOrDefaultAsync(x => x.Id == request.AccountId, cancellationToken);

            if (entity != null)
            {
                entity.UpdateLevyStatus(ApprenticeshipEmployerType.Levy);
                await _db.Value.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"LevyStatus set to Levy for AccountId : {request.AccountId}");
            }
        }
    }
}
