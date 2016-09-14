using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.ProviderApprenticeshipsService.Domain;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class SubmitCommitmentViewModel
    {
        public Commitment Commitment { get; set; }
        public SubmitCommitmentModel SubmitCommitmentModel { get; set; }
    }
}