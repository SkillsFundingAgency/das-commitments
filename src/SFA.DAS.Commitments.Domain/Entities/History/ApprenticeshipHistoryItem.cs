using System;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class ApprenticeshipHistoryItem
    {
        public long ApprenticeshipId { get; set; }

        public long UserId { get; set; }

        public CallerType UpdatedByRole { get; set; }

        public ApprenticeshipChangeType ChangeType { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}