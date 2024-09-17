using SFA.DAS.Apprenticeships.Types.Enums;
using System;

namespace SFA.DAS.Apprenticeships.Types
{
    public class ApprenticeshipPriceChangedEvent : ApprenticeshipEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public ApprovedBy ApprovedBy { get; set; }
        public string ProviderApprovedBy { get; set; }
        public string EmployerApprovedBy { get; set; }
    }
}
