using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IOverlappingTrainingDateRequestDomainService
    {
        Task<OverlappingTrainingDateRequest> CreateOverlappingTrainingDateRequest(long apprenticeshipId,
         UserInfo userInfo, CancellationToken cancellationToken);
    }
}
