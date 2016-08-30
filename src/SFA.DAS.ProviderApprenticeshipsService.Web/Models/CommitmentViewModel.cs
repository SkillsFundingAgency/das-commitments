using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class CommitmentViewModel
    {
        public List<CommitmentListItem> Commitments { get; set; }
    }
}