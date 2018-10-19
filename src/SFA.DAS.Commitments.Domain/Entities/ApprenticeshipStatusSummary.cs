namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipStatusSummary
    {
        public string LegalEntityIdentifier { get; set; }
        public SFA.DAS.Common.Domain.Types.OrganisationType LegalEntityOrganisationType { get; set; }

        public int PendingApprovalCount { get; set; }
        public int ActiveCount { get; set; }
        public int PausedCount { get; set; }
        public int WithdrawnCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
