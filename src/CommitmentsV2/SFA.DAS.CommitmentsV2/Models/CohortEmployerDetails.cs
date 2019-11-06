namespace SFA.DAS.CommitmentsV2.Models
{
    public class CohortEmployerDetails
    {
        public AccountLegalEntity AccountLegalEntity { get; }
        public Account TransferSenderAccount { get; }

        public CohortEmployerDetails(AccountLegalEntity accountLegalEntity, Account transferSenderAccount)
        {
            AccountLegalEntity = accountLegalEntity;
            TransferSenderAccount = transferSenderAccount;
        }

    }
}