namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class SubmitCommitmentModel
    {
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
        public string Message { get; set; }
    }
}