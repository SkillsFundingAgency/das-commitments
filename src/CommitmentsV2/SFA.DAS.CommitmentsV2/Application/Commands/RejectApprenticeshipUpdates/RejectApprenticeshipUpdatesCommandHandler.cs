using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RejectApprenticeshipUpdates
{
    public class RejectApprenticeshipUpdatesCommandHandler : IRequestHandler<RejectApprenticeshipUpdatesCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<RejectApprenticeshipUpdatesCommandHandler> _logger;

        public RejectApprenticeshipUpdatesCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            IAuthenticationService authenticationService,
            ILogger<RejectApprenticeshipUpdatesCommandHandler> logger)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public async Task Handle(RejectApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RejectApprenticeshipUpdatesCommand received from ApprenticeshipId :" + command.ApprenticeshipId);
            var party = _authenticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
            CheckPartyIsValid(party, command, apprenticeship);

            if (apprenticeship.ApprenticeshipUpdate.FirstOrDefault(x => x.Status == ApprenticeshipUpdateStatus.Pending) == null)
            {
                throw new InvalidOperationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
            }

            apprenticeship.RejectApprenticeshipUpdate(party, command.UserInfo);
        }

        private static void CheckPartyIsValid(Party party, RejectApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship)
        {
            if (party == Party.Employer && command.AccountId != apprenticeship.Cohort.EmployerAccountId)
            {
                throw new InvalidOperationException($"Employer {command.AccountId} not authorised to update apprenticeship {apprenticeship.Id}");
            }
        }
    }
}
