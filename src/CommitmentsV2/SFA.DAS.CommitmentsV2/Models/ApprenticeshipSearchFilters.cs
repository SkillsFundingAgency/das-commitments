using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipSearchFilters
    {
        public string SearchTerm { get; set; }
        public string EmployerName { get; set; }
        public string ProviderName { get; set; }
        public string CourseName { get; set; }
        public ApprenticeshipStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AccountLegalEntityId { get; set; }
        public DateRange StartDateRange { get; set; }
        public Alerts? Alert { get; set; }
        public ConfirmationStatus? ApprenticeConfirmationStatus { get; set; }
        public DeliveryModel? DeliveryModel { get; set; }
    }
}
