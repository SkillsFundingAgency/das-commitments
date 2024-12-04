using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;

public class CreateOverlappingTrainingDateRequestCommand : IRequest<CreateOverlappingTrainingDateResult>
{
    public long ProviderId { get; set; }
    public long DraftApprenticeshipId { get; set; }
    public UserInfo UserInfo { get; set; }
}