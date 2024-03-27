using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UndoApprenticeshipUpdates;

public class UndoApprenticeshipUpdatesCommandHandler : IRequestHandler<UndoApprenticeshipUpdatesCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<UndoApprenticeshipUpdatesCommandHandler> _logger;

    public UndoApprenticeshipUpdatesCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
        IAuthenticationService authenticationService,
        ILogger<UndoApprenticeshipUpdatesCommandHandler> logger)
    {
        _dbContext = dbContext;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task Handle(UndoApprenticeshipUpdatesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UndoApprenticeshipUpdatesCommand received from ApprenticeshipId :{Id}", command.ApprenticeshipId);
        var party = _authenticationService.GetUserParty();
        var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
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