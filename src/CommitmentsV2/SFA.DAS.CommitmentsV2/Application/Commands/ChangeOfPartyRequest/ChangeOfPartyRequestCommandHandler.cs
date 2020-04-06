using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CosmosDb;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest
{
    public class ChangeOfPartyRequestCommandHandler : AsyncRequestHandler<ChangeOfPartyRequestCommand>
    {
        private readonly ILogger<ChangeOfPartyRequestCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ChangeOfPartyRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<ChangeOfPartyRequestCommandHandler> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        protected override async Task Handle(ChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var db = _dbContext.Value;

                var apprenticeship = await db.Apprenticeships.SingleAsync(x => x.Id == command.ApprenticeshipId, cancellationToken: cancellationToken);

                var changeOfPartyRequest = new Models.ChangeOfPartyRequest(apprenticeship,
                    command.ChangeOfPartyRequestType, command.Party, command.PartyId,
                    command.NewPrice.Value, command.NewStartDate.Value, null,           // NOTE: I know the NewPrice and NewStartDate cannot be NULL at the moment, but it may be null when an Employer makes teh command
                    command.UserInfo, DateTime.UtcNow);

                if (await db.ChangeOfPartyRequests.AnyAsync(x => x.ApprenticeshipId == command.ApprenticeshipId && (x.Status == ChangeOfPartyRequestStatus.Pending || x.Status == ChangeOfPartyRequestStatus.Approved)))
                {
                    throw new InvalidOperationException(
                        $"Apprenticeship {command.ApprenticeshipId} already has a ChangeOfPartyRequest with a status of Pending or Approved");
                }

                db.ChangeOfPartyRequests.Add(changeOfPartyRequest);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Adding ChangeOfPartyRequest");
                throw;
            }
        }
    }
}