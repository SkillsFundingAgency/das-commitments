namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        void Create(Commitment commitment);
    }
}