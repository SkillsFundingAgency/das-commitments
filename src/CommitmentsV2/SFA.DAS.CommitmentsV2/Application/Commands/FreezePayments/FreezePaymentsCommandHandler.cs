using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.FreezePayments;

public class FreezePaymentsCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService)
    : IRequestHandler<FreezePaymentsCommand>
{
    public async Task Handle(FreezePaymentsCommand command, CancellationToken cancellationToken)
    {
        var party = authenticationService.GetUserParty();
        CheckPartyIsValid(party);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
        if (!command.FreezePaymentsReason.HasValue)
        {
            throw new DomainException(nameof(command.FreezePaymentsReason), "A reason for pausing payments must be provided");
        }

        apprenticeship.FreezePayments(currentDate, party, command.UserInfo, command.FreezePaymentsReason.Value);

        // APPMAN-2645: after StoreLearningHistoryCommand merges to main, call SaveChangesAsync then send history (see StopApprenticeshipCommandHandler), e.g.:
        // await dbContext.Value.SaveChangesAsync(cancellationToken);
        // await messageSession.Send(new StoreLearningHistoryCommand { ... });
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Only employers are allowed to freeze payments - {party} is invalid");
        }
    }
}
