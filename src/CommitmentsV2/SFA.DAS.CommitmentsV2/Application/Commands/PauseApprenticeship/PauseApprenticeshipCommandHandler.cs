using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;

public class PauseApprenticeshipCommandHandler : IRequestHandler<PauseApprenticeshipCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ICurrentDateTime _currentDate;
    private readonly IAuthenticationService _authenticationService;

    public PauseApprenticeshipCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
        ICurrentDateTime currentDate,
        IAuthenticationService authenticationService)
    {
        _dbContext = dbContext;
        _currentDate = currentDate;
        _authenticationService = authenticationService;
    }

    public async Task Handle(PauseApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        var party = _authenticationService.GetUserParty();
        CheckPartyIsValid(party);

        var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
        apprenticeship.PauseApprenticeship(_currentDate, party, command.UserInfo);
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
        }
    }
}