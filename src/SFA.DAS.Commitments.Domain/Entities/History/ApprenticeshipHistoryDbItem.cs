using System;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class ApprenticeshipHistoryDbItem
    {
        public long ApprenticeshipId { get; set; }

        public long UserId { get; set; }

        public UserRole UpdatedByRole { get; set; }

        public ApprenticeshipChangeType ChangeType { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}