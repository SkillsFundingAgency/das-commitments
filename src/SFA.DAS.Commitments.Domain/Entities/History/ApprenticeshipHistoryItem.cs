namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class ApprenticeshipHistoryItem
    {
        public long ApprenticeshipId { get; set; }

        public string UserId { get; set; }

        public CallerType UpdatedByRole { get; set; }
    }
}