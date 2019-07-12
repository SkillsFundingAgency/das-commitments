using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetCohortResponse
    {
        public long CohortId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string ProviderName { get; set; }
        public bool IsFundedByTransfer { get; set; }
        public Party WithParty { get; set; }
    }
}