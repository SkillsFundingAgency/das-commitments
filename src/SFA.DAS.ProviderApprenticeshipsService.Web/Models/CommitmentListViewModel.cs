using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class CommitmentListViewModel
    {
        public int NumberOfTasks { get; set; }
        public List<CommitmentListItem> Commitments { get; set; }
    }
}