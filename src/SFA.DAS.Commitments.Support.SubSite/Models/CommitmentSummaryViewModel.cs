namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class CommitmentSummaryViewModel
    {
        public CommitmentSummaryViewModel()
        {
            ReponseMessages = new List<string>();
        }
        public string CohortReference { get; set; }
        public string EmployerName { get; set; }
        public string ProviderName  { get; set; }
        public string HashedAccountId { get; set; }
        public long? ProviderUkprn { get; set; }
        public string CohortStatusText { get; set; }
        public List<string> ReponseMessages { get; set; }
        public bool HasError => ReponseMessages != null && ReponseMessages.Any();
    }
}