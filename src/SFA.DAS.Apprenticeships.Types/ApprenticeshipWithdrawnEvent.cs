using System;

namespace SFA.DAS.Apprenticeships.Types
{
    public class ApprenticeshipWithdrawnEvent
    {
        public Guid ApprenticeshipKey { get; set; }
        public long ApprenticeshipId { get; set; }
        public string Reason { get; set; }
        public DateTime LastDayOfLearning { get; set; }
        public long EmployerAccountId { get; set; }
    }
}
