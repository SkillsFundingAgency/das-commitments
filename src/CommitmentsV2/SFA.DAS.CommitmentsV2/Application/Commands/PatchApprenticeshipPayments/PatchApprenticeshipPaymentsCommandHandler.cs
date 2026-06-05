using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PatchApprenticeshipPayments;

public class PatchApprenticeshipPaymentsCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService)
    : IRequestHandler<PatchApprenticeshipPaymentsCommand>
{
    public async Task Handle(PatchApprenticeshipPaymentsCommand command, CancellationToken cancellationToken)
    {
        var party = authenticationService.GetUserParty();
        CheckPartyIsValid(party, command.FreezePayments);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

        if (command.FreezePayments)
        {
            if (!command.FreezePaymentsReason.HasValue)
            {
                throw new DomainException(nameof(command.FreezePaymentsReason), "A reason for pausing payments must be provided");
            }

            apprenticeship.FreezePayments(currentDate, party, command.UserInfo, command.FreezePaymentsReason.Value);
        }
        else
        {
            apprenticeship.UnfreezePayments(currentDate, party, command.UserInfo);
        }

        // APPMAN-2645: after StoreLearningHistoryCommand merges to main, call SaveChangesAsync then send history (see StopApprenticeshipCommandHandler), e.g.:
        // await dbContext.Value.SaveChangesAsync(cancellationToken);
        // await messageSession.Send(new StoreLearningHistoryCommand { ... });
    }

    private static void CheckPartyIsValid(Party party, bool freezePayments)
    {
        if (party != Party.Employer)
        {
            var action = freezePayments ? "freeze" : "unfreeze";
            throw new DomainException(nameof(party), $"Only employers are allowed to {action} payments - {party} is invalid");
        }
    }
}
