using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Configuration;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class CommitmentRepository : BaseRepository, ICommitmentRepository
    {
        public CommitmentRepository(CommitmentConfiguration configuration) : base(configuration)
        {
        }

        public void Create(Commitment commitment)
        {
            throw new System.NotImplementedException();
        }
    }
}