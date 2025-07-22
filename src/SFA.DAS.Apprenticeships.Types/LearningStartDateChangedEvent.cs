using System;
using SFA.DAS.Learning.Types.Models;

namespace SFA.DAS.Learning.Types
{
    public class LearningStartDateChangedEvent : LearningEvent
    {
        public long ApprovalsApprenticeshipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ProviderApprovedBy { get; set; }
        public string EmployerApprovedBy { get; set; }
        public string Initiator { get; set; }
    }
}
