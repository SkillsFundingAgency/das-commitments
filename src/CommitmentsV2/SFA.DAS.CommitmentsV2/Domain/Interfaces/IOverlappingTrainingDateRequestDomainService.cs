using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    internal interface IOverlappingTrainingDateRequestDomainService
    {
        Task<OverlappingTrainingDateRequest> CreateOverlappingTrainingDateRequest(long apprenticeshipId,
          long previousApprenticeshipId,UserInfo userInfo, CancellationToken cancellationToken);
    }
}
