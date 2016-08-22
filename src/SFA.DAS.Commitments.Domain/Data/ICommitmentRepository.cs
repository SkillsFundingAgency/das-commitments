using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task Create(Commitment commitment);
    }
}