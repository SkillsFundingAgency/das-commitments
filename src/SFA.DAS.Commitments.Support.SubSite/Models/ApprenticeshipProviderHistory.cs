namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ApprenticeshipProviderHistoryViewModel
    {
        public long ApprenticeshipId { get; set; }
        public string ProviderName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}