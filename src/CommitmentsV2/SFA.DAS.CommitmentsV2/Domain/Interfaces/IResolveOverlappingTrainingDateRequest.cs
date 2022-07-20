using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IResolveOverlappingTrainingDateRequestService
    {
        Task ResolveByApprenticeship(long apprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType);
        Task ResolveByDraftApprenticeshp(long draftAppretniceshipId, OverlappingTrainingDateRequestResolutionType resolutionType);
    }
}
