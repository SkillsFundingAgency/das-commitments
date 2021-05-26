using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetCohortResponse
    {
        public long CohortId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string ProviderName { get; set; }
        public bool IsFundedByTransfer => TransferSenderId != null;
        public long? TransferSenderId { get; set; }
        public Party WithParty { get; set; }
        public string LatestMessageCreatedByEmployer { get; set; }
        public string LatestMessageCreatedByProvider { get; set; }
        public bool IsApprovedByEmployer { get; set; }
        public bool IsApprovedByProvider { get; set; }
        public bool IsCompleteForEmployer { get; set; }
        public bool IsCompleteForProvider { get; set; }
		public ApprenticeshipEmployerType LevyStatus { get; set; }
        public long? ChangeOfPartyRequestId { get; set; }
        public bool IsLinkedToChangeOfPartyRequest => ChangeOfPartyRequestId.HasValue;
    }
}