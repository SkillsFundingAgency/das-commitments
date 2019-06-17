namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryResponse
    {
        public long CohortId { get; set; }
        public string LegalEntityName { get; set; }

        /// <summary>
        ///     Indicates whether the cohort is funding by a transfer. Transfer funded cohorts cannot
        ///     use framework courses.
        /// </summary>
        public bool IsFundedByTransfer { get; set; }
    }
}
