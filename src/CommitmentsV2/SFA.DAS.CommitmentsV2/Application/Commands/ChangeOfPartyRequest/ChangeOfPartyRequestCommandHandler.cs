using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest
{
    public class ChangeOfPartyRequestCommandHandler : AsyncRequestHandler<ChangeOfPartyRequestCommand>
    {
        private readonly ILogger<ChangeOfPartyRequestCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public ChangeOfPartyRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, ILogger<ChangeOfPartyRequestCommandHandler> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
            _authenticationService = authenticationService;
        }

        protected override async Task Handle(ChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var db = _dbContext.Value;

                var apprenticeship = await db.Apprenticeships.Include(x=>x.Cohort).SingleAsync(x => x.Id == command.ApprenticeshipId, cancellationToken: cancellationToken);

                Party originatingParty;
                originatingParty= _authenticationService.GetUserParty();

                var changeOfPartyRequest = new Models.ChangeOfPartyRequest(apprenticeship,
                    command.ChangeOfPartyRequestType, originatingParty, command.PartyId,
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