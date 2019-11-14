using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class CohortSummary
    {
        public long AccountId { get; set; }
        public string EmployerName { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long CohortId { get; set; }
        public int NumberOfDraftApprentices { get; set; }
        public string LastMessageFromProvider { get; set; }
        public string LastMessageFromEmployer { get; set; }
        public bool IsDraft { get; set; }
        public Party WithParty { get; set; }
    }
}