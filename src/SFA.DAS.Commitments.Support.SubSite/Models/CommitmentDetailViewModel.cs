namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class CommitmentDetailViewModel
    {
       public  CommitmentSummaryViewModel CommitmentSummary { get; set; }
        public IEnumerable<ApprenticeshipSearchItemViewModel> CommitmentApprenticeships{ get; set; }
    }
}