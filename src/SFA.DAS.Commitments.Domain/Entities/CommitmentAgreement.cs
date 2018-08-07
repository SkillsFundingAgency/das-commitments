
namespace SFA.DAS.Commitments.Domain.Entities
{
    /// <summary>
    /// A subset of Commitment related to agreements
    /// </summary>
    public class CommitmentAgreement
    {
        //public long Id { get; set; }    // todo: ?? prob not
        public string Reference { get; set; }
        public string LegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}
