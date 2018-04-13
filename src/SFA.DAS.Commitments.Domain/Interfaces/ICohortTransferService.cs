using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface ICohortTransferService
    {
        Task ResetCommitmentTransferRejection(Commitment commitment, string userId, string userName);
    }
}
