﻿
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;

public class AcceptApprenticeshipUpdatesCommandHandler : IRequestHandler<AcceptApprenticeshipUpdatesCommand>
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

    public async Task Handle(AcceptApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AcceptApprenticeshipUpdatesCommand received from ApprenticeshipId : {Id}", command.ApprenticeshipId);

        var party = GetParty(command);
        var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
        CheckPartyIsValid(party, command, apprenticeship);

        if (apprenticeship.ApprenticeshipUpdate.FirstOrDefault(x => x.Status == ApprenticeshipUpdateStatus.Pending) == null)
        {
            throw new InvalidOperationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
        }

        var apprenticeshipUpdate = apprenticeship.ApprenticeshipUpdate.First(x => x.Status == ApprenticeshipUpdateStatus.Pending);

        await CheckUlnOverlap(command, apprenticeship, apprenticeshipUpdate, cancellationToken);
        if (apprenticeshipUpdate.Email != null)
        {
            if (apprenticeship.EmailAddressConfirmed == true)
            {
                throw new DomainException("ApproveChanges", "Unable to approve these changes, as the apprentice has confirmed their email address");
            }

            await CheckEmailOverlap(command, apprenticeship, apprenticeshipUpdate, cancellationToken);
        }
        apprenticeship.ApplyApprenticeshipUpdate(party, command.UserInfo, _dateTimeService);
    }

		private Party GetParty(AcceptApprenticeshipUpdatesCommand command)
		{
			if (_authenticationService.AuthenticationServiceType == AuthenticationServiceType.MessageHandler)
			{
				return command.Party;
			}

			return _authenticationService.GetUserParty();
		}

    private async Task CheckUlnOverlap(AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate apprenticeshipUpdate, CancellationToken cancellationToken)
    {
        var overlapCheckResult = await _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, 
            new Domain.Entities.DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value),
            command.ApprenticeshipId,
            cancellationToken);

        if (overlapCheckResult.HasOverlaps)
        {
            throw new DomainException("ApprenticeshipId",
                "Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
        }
    }

    private async Task CheckEmailOverlap(AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate apprenticeshipUpdate, CancellationToken cancellationToken)
    {
        var overlapCheckResult = await _overlapCheckService.CheckForEmailOverlaps(apprenticeshipUpdate.Email,
            new Domain.Entities.DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value),
            command.ApprenticeshipId,
            null,
            cancellationToken);

        if (overlapCheckResult != null)
        {
            throw new DomainException("ApprenticeshipId", overlapCheckResult.BuildErrorMessage());
        }
    }

    private static void CheckPartyIsValid(Party party, AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship)
    {
        if (party == Party.Employer && command.AccountId != apprenticeship.Cohort.EmployerAccountId)
        {
            throw new InvalidOperationException($"Employer {command.AccountId} not authorised to update apprenticeship {apprenticeship.Id}");
        }
    }
}