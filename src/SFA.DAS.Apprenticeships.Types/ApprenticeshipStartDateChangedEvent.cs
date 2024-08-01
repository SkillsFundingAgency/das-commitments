using System;
using SFA.DAS.Apprenticeships.Types.Models;

namespace SFA.DAS.Apprenticeships.Types
{
    public class ApprenticeshipStartDateChangedEvent : EpisodeUpdatedEvent
    {
        public Guid ApprenticeshipKey { get; set; }
        public long ApprenticeshipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ProviderApprovedBy { get; set; }
        public string EmployerApprovedBy { get; set; }
        public string Initiator { get; set; }
    }
}
