using System;
using SFA.DAS.Learning.Types.Enums;
using SFA.DAS.Learning.Types.Models;

namespace SFA.DAS.Learning.Types
{
    public class LearningPriceChangedEvent : LearningEvent
    {
        public long ApprovalsApprenticeshipId { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public ApprovedBy ApprovedBy { get; set; }
        public string ProviderApprovedBy { get; set; }
        public string EmployerApprovedBy { get; set; }
    }
}
