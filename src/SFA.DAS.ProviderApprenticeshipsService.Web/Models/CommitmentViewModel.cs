using System.Collections.Generic;
using SFA.DAS.ProviderApprenticeshipsService.Domain;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class CommitmentViewModel
    {
        public List<CommitmentView> Commitments { get; set; }
    }
}