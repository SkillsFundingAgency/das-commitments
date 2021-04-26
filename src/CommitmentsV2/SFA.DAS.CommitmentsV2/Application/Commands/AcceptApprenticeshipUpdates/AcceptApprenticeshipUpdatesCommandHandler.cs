using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates
{
    public class AcceptApprenticeshipUpdatesCommandHandler : AsyncRequestHandler<AcceptApprenticeshipUpdatesCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly ILogger<AcceptApprenticeshipUpdatesCommandHandler> _logger;

        public AcceptApprenticeshipUpdatesCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            IOverlapCheckService overlapCheckService,
            ILogger<AcceptApprenticeshipUpdatesCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _overlapCheckService = overlapCheckService;
            _logger = logger;
        }

        protected override async Task Handle(AcceptApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
            CheckPartyIsValid(party, command, apprenticeship);

            if (apprenticeship.ApprenticeshipUpdate.Count == 0)
            {
                throw new InvalidOperationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
            }

            var apprenticeshipUpdate = apprenticeship.ApprenticeshipUpdate.First();

           var overlapCheckResult = await _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, new Domain.Entities.DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value), command.ApprenticeshipId, cancellationToken);

            if (overlapCheckResult.HasOverlaps)
            {
                //TODO : Which property name should we use.
                throw new DomainException("StartDate",  "Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }

            apprenticeship.ApplyApprenticeshipUpdate(party, command.UserInfo);

            //apprenticeship.PauseApprenticeship(_currentDate, party, command.UserInfo);
        }

        private void CheckPartyIsValid(Party party, AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
            }

            if (party == Party.Employer && command.AccountId != apprenticeship.Cohort.EmployerAccountId)
            {
                throw new InvalidOperationException($"Employer {command.AccountId} not authorised to update apprenticeship {apprenticeship.Id}");
            }
        }
    }
}
