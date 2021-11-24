using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    // This is a pseudo-entity to represent the result of the [GetApprenticeshipStatusSummaries] stored proc, it's not a table in the database.
    public class ApprenticeshipStatusSummary
    {        
        public string LegalEntityId { get; set; }
        public Common.Domain.Types.OrganisationType LegalEntityOrganisationType { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int Count { get; set; }
    }
}
