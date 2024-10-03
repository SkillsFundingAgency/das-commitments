using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UndoApprenticeshipUpdates;

public class UndoApprenticeshipUpdatesCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IAuthenticationService authenticationService,
    ILogger<UndoApprenticeshipUpdatesCommandHandler> logger)
    : IRequestHandler<UndoApprenticeshipUpdatesCommand>
{
    public async Task Handle(UndoApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("UndoApprenticeshipUpdatesCommand received from ApprenticeshipId :{Id}", command.ApprenticeshipId);
        
        var party = authenticationService.GetUserParty();
        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
        
        CheckPartyIsValid(party, command, apprenticeship);

        if (apprenticeship.ApprenticeshipUpdate.All(x => x.Status != ApprenticeshipUpdateStatus.Pending))
        {
            throw new InvalidOperationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");
        }

        apprenticeship.UndoApprenticeshipUpdate(party, command.UserInfo);
    }

    private static void CheckPartyIsValid(Party party, UndoApprenticeshipUpdatesCommand command, Apprenticeship apprenticeship)
    {
        if (party == Party.Employer && command.AccountId != apprenticeship.Cohort.EmployerAccountId)
        {
            throw new InvalidOperationException($"Employer {command.AccountId} not authorised to update apprenticeship {apprenticeship.Id}");
        }
    }
}