
namespace SFA.DAS.Commitments.Domain.Entities
{
    /// <summary>
    /// A subset of Commitment related to agreements
    /// </summary>
    public class CommitmentAgreement
    {
        public string Reference { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
