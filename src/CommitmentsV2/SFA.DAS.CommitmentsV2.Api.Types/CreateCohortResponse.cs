namespace SFA.DAS.CommitmentsV2.Api.Types
{
    // Do we need to return encoded values and should we include the AccountId? 
    public sealed class CreateCohortResponse
    {
        public int CohortId { get; set; }

        public int DraftApprenticeshipId { get; set; }
    }
}