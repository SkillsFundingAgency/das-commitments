
namespace SFA.DAS.Commitments.Api.Types.Commitment
{
    /// <summary>
    /// A subset of Commitment related to agreements
    /// </summary>
    public sealed class CommitmentAgreement
    {
        public string Reference { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
