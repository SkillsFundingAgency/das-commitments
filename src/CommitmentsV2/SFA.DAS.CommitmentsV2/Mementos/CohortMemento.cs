using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mementos
{
    public class CohortMemento : IMemento
    {
        public long Id { get; }
        public string CohortReference { get; }
        public long ProviderId { get; set; }
        public long EmployerAccountId { get; set; }
        public Party WithParty { get; }
        public Party Approvals { get; }
        public long? TransferSenderId { get; }
        
        public CohortMemento(long id, string cohortReference, long providerId, long employerAccountId, Party withParty, Party approvals, long? transferSenderId)
        {
            Id = id;
            CohortReference = cohortReference;
            ProviderId = providerId;
            EmployerAccountId = employerAccountId;
            WithParty = withParty;
            Approvals = approvals;
            TransferSenderId = transferSenderId;
        }
    }
}
