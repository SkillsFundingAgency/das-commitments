using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;

public class PauseApprenticeshipCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService,
    IMessageSession messageSession)
    : IRequestHandler<PauseApprenticeshipCommand>
{
    public async Task Handle(PauseApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        var party = authenticationService.GetUserParty();
        CheckPartyIsValid(party);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
        var appliedDate = currentDate.UtcNow;
        var previousStatus = apprenticeship.GetApprenticeshipStatus(appliedDate);
        apprenticeship.PauseApprenticeship(currentDate, party, command.UserInfo);

        await apprenticeship.SendStatusChangeHistory(previousStatus, appliedDate, LearningSourceType.ManualChange,
            history => messageSession.Send(history), userInfo: command.UserInfo);
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
        }
    }
}