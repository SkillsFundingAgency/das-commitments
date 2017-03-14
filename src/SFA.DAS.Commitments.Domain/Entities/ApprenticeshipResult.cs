using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipResult
    {
        public long Id { get; set; }
        public string Uln { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityName { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
    }
}
