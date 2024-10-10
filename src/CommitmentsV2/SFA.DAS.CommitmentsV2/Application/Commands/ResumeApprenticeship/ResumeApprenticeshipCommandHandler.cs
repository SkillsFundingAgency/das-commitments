using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;

public class ResumeApprenticeshipCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService,
    ILogger<ResumeApprenticeshipCommandHandler> logger)
    : IRequestHandler<ResumeApprenticeshipCommand>
{
    public async Task Handle(ResumeApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var party = authenticationService.GetUserParty();
            CheckPartyIsValid(party);

            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
            apprenticeship.ResumeApprenticeship(currentDate, party, command.UserInfo);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Resuming Apprenticeship with id {ApprenticeshipId}", command.ApprenticeshipId);
            throw;
        }
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
        }
    }
}