using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest
{
    public class CreateOverlappingTrainingDateRequestCommand : IRequest
    {
        public long ApprneticeshipId { get; set; }
        public long PreviousApprenticeshipId { get; set; }
    }
}
