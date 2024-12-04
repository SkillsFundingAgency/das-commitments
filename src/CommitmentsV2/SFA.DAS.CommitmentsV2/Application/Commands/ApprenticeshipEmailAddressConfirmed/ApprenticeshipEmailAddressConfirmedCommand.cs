namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

public class ApprenticeshipEmailAddressConfirmedCommand : IRequest
{
    public Guid ApprenticeId { get; set; }
    public long ApprenticeshipId { get; set; }

    public ApprenticeshipEmailAddressConfirmedCommand(Guid apprenticeId, long apprenticeshipId)
    {
        ApprenticeId = apprenticeId;
        ApprenticeshipId = apprenticeshipId;
    }
}