using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.Core.Models
{
    public class CommitmentDetailViewModel
    {
       public  CommitmentSummaryViewModel CommitmentSummary { get; set; }
        public IEnumerable<ApprenticeshipSearchItemViewModel> CommitmentApprenticeships{ get; set; }
    }
}