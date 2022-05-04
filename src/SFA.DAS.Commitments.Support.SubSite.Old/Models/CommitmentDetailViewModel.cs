using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class CommitmentDetailViewModel
    {
       public  CommitmentSummaryViewModel CommitmentSummary { get; set; }
        public IEnumerable<ApprenticeshipSearchItemViewModel> CommitmentApprenticeships{ get; set; }
    }
}