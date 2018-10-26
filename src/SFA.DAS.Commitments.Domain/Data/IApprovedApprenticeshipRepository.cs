using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprovedApprenticeshipRepository
    {
        Task<ApprovedApprenticeship> Get(long id);
    }
}
