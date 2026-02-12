using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using DateRange = SFA.DAS.CommitmentsV2.Domain.Entities.DateRange;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;

public class AcceptApprenticeshipUpdatesCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IAuthenticationService authenticationService,
    IOverlapCheckService overlapCheckService,
    ICurrentDateTime dateTimeService,
    ILogger<AcceptApprenticeshipUpdatesCommandHandler> logger)
    : IRequestHandler<AcceptApprenticeshipUpdatesCommand>
{
    public async Task Handle(AcceptApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("AcceptApprenticeshipUpdatesCommand received from ApprenticeshipId : {Id}", command.ApprenticeshipId);

        var party = GetParty(command);
        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
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
        var standard = dbContext.Value.Standards.FirstOrDefault(x => x.StandardUId == apprenticeship.StandardUId);
        apprenticeship.ApplyApprenticeshipUpdate(party, command.UserInfo, dateTimeService, standard?.ApprenticeshipType);
    }

    private Party GetParty(AcceptApprenticeshipUpdatesCommand command)
        {
            return authenticationService.AuthenticationServiceType == AuthenticationServiceType.MessageHandler 
                ? command.Party 
                : authenticationService.GetUserParty();
        }

    private async Task CheckUlnOverlap(AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate apprenticeshipUpdate, CancellationToken cancellationToken)
    {
        var overlapCheckResult = await overlapCheckService.CheckForOverlaps(apprenticeship.Uln, 
            new DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value),
            command.ApprenticeshipId,
            cancellationToken);

        if (overlapCheckResult.HasOverlaps)
        {
            throw new DomainException("ApprenticeshipId", "Unable to create ApprenticeshipUpdate due to overlapping apprenticeship");
        }
    }

    private async Task CheckEmailOverlap(AcceptApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship, ApprenticeshipUpdate apprenticeshipUpdate, CancellationToken cancellationToken)
    {
        var overlapCheckResult = await overlapCheckService.CheckForEmailOverlaps(apprenticeshipUpdate.Email,
            new DateRange(apprenticeshipUpdate.StartDate ?? apprenticeship.StartDate.Value, apprenticeshipUpdate.EndDate ?? apprenticeship.EndDate.Value),
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