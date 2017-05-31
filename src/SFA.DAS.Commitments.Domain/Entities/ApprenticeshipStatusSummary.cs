using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipStatusSummary
    {
        public int PendingApprovalCount { get; set; }
        public int ActiveCount { get; set; }
        public int PausedCount { get; set; }
        public int WithdrawnCount { get; set; }
        public int CompletedCount { get; set; }
        public int DeletedCount { get; set; }
    }
}
