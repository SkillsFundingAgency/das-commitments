using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IResolveOverlappingTrainingDateRequestService
    {
        Task Resolve(long? apprenticeshipId, long? draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType);
        Task DraftApprenticeshpDeleted(long draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType);
    }
}
