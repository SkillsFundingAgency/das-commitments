using System.Collections.Generic;
using SFA.DAS.ProviderApprenticeshipsService.Domain;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class ExtendedApprenticeshipViewModel
    {
        public ApprenticeshipViewModel Apprenticeship { get; set; }
        public List<Standard> Standards { get; set; }
    }

    public class ApprenticeshipViewModel
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public long ProviderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ULN { get; set; }
        public string TrainingId { get; set; } //standard or framework
        public decimal? Cost { get; set; }
        public int? StartMonth { get; set; }
        public int? StartYear { get; set; }
        public int? EndMonth { get; set; }
        public int? EndYear { get; set; }
        public string Status { get; set; }
        public string AgreementStatus { get; set; }
    }
}