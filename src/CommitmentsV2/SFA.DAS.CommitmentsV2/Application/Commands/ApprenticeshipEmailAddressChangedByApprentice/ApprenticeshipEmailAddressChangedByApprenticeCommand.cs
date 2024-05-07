namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice
{
    public class ApprenticeshipEmailAddressChangedByApprenticeCommand : IRequest
    {
        public Guid ApprenticeId { get; set; }
        public long ApprenticeshipId { get; set; }

        public ApprenticeshipEmailAddressChangedByApprenticeCommand(Guid apprenticeId, long apprenticeshipId)
        {
            ApprenticeId = apprenticeId;
            ApprenticeshipId = apprenticeshipId;
        }
    }
}