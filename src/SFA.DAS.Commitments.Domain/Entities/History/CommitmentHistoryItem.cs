using System;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class CommitmentHistoryItem
    {
        public long CommitmentId { get; set; }

        public long UserId { get; set; }

        public CallerType UpdatedByRole { get; set; }

        public CommitmentChangeType ChangeType { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}