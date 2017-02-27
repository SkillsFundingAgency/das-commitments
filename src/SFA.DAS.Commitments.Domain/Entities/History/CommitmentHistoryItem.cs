namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class CommitmentHistoryItem
    {
        public long CommitmentId { get; set; }

        public string UserId { get; set; }

        public CallerType UpdatedByRole { get; set; }
    }
}