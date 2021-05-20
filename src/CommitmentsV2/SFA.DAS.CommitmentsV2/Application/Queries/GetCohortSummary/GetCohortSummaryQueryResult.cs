using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryQueryResult
    {
        public long CohortId { get; set; }
        public string CohortReference { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }

        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }

        /// <summary>
        ///     Indicates whether the cohort is funding by a transfer. Transfer funded cohorts cannot
        ///     use framework courses.
        /// </summary>
        public bool IsFundedByTransfer => TransferSenderId != null;
        public long? TransferSenderId { get; set; }
        public string TransferSenderName { get; set; }

        public Party WithParty { get; set; }
        public string LatestMessageCreatedByEmployer { get; set; }
        public string LatestMessageCreatedByProvider { get; set; }
        public LastAction LastAction { get; set; }
        public string LastUpdatedByEmployerEmail { get; set; }
        public string LastUpdatedByProviderEmail { get; set; }
		public bool IsApprovedByProvider { get; set; }
        public bool IsApprovedByEmployer { get; set; }
        public bool IsCompleteForEmployer { get; set; }
        public bool IsCompleteForProvider { get; set; }
        public Party Approvals { get; set; }
        public ApprenticeshipEmployerType LevyStatus { get; set; }
        public long? ChangeOfPartyRequestId { get; set; }
    }
}