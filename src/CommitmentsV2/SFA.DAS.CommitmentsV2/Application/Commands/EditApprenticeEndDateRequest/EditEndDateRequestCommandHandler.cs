using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;

public class EditEndDateRequestCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService,
    IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    : IRequestHandler<EditEndDateRequestCommand>
{
    public async Task Handle(EditEndDateRequestCommand command, CancellationToken cancellationToken)
    {
        var party = authenticationService.GetUserParty();

        CheckPartyIsValid(party);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

        apprenticeship.EditEndDateOfCompletedRecord(command.EndDate.Value, currentDate, party, command.UserInfo);

        await resolveOverlappingTrainingDateRequestService.Resolve(
            command.ApprenticeshipId,
            null,
            OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateUpdate
        );
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
        }
    }
}