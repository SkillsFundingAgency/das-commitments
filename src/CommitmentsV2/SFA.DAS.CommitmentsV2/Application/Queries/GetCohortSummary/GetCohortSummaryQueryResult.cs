using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryResult
    {
        public long CohortId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }

        public string ProviderName { get; set; }

        /// <summary>
        ///     Indicates whether the cohort is funding by a transfer. Transfer funded cohorts cannot
        ///     use framework courses.
        /// </summary>
        public bool IsFundedByTransfer => TransferSenderId != null;
        public long? TransferSenderId { get; set; }

        public Party WithParty { get; set; }
        public string LatestMessageCreatedByEmployer { get; set; }
        public string LatestMessageCreatedByProvider { get; set; }
        public EditStatus EditStatus { get; set; }
    }
}