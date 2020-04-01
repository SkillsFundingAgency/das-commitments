using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.SetLevyStatusToLevy
{
    public class SetLevyStatusToLevyCommandHandler : AsyncRequestHandler<SetLevyStatusToLevyCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ILogger<SetLevyStatusToLevyCommandHandler> _logger;


        public SetLevyStatusToLevyCommandHandler(Lazy<SetLevyStatusToLevyCommandHandler> db, ILogger<SetLevyStatusToLevyCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        protected override async Task Handle(SetLevyStatusToLevyCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Value.Accounts.SingleAsync(x => x.Id == request.AccountId, cancellationToken);

            if (entity != null)
            {
                entity.SetLevyStatusToLevy();
                await _db.Value.SaveChangesAsync();
                _logger.LogInformation($"LevyStatus set to Levy for AccountId : {request.AccountId}");
            }
        }
    }
}
