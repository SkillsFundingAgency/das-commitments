using SFA.DAS.CommitmentsV2.Types;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IResolveOverlappingTrainingDateRequestService
    {
        Task Resolve(long apprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType);
    }
}
