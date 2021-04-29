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
using SFA.DAS.NServiceBus.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates
{
    public class AcceptApprenticeshipUpdatesCommandHandler : AsyncRequestHandler<AcceptApprenticeshipUpdatesCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly ICurrentDateTime _dateTimeService;
        private readonly ILogger<AcceptApprenticeshipUpdatesCommandHandler> _logger;

        public AcceptApprenticeshipUpdatesCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            IAuthenticationService authenticationService,
            IOverlapCheckService overlapCheckService,
            ICurrentDateTime dateTimeService,
            ILogger<AcceptApprenticeshipUpdatesCommandHandler> logger)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _overlapCheckService = overlapCheckService;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        protected override async Task Handle(AcceptApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AcceptApprenticeshipUpdatesCommand received from ApprenticeshipId :" + command.ApprenticeshipId);

            var party = _authenticationService.GetUserParty();
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
            CheckPartyIsValid(party, command, apprenticeship);

            if (apprenticeship.ApprenticeshipUpdate.FirstOrDefault(x => x.Status == ApprenticeshipUpdateStatus.Pending) == null)
            {
                throw new InvalidOperationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
            }

            var apprenticeshipUpdate = apprenticeship.ApprenticeshipUpdate.First(x => x.Status == ApprenticeshipUpdateStatus.Pending);

           var overlapCheckResult = await _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, 
               new Domain.Entities.DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value), 
               command.ApprenticeshipId, 
               cancellationToken);

            if (overlapCheckResult.HasOverlaps)
            {
                throw new DomainException("StartDate",  "Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
            }

            apprenticeship.ApplyApprenticeshipUpdate(party, command.UserInfo, _dateTimeService);
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
