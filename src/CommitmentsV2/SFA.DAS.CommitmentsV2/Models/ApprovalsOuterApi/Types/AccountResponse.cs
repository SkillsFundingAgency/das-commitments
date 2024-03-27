namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types
{
    public class AccountResponse
    {
        public long AccountId { get; set; }
        public string HashedAccountId { get; set; }
        public string PublicHashedAccountId { get; set; }
        public string DasAccountName { get; set; }
        public DateTime DateRegistered { get; set; }
        public string OwnerEmail { get; set; }
        public decimal Balance { get; set; }
        public decimal RemainingTransferAllowance { get; set; }
        public decimal StartingTransferAllowance { get; set; }
        public int AccountAgreementType { get; set; }
        public string ApprenticeshipEmployerType { get; set; }
        public bool IsAllowedPaymentOnService { get; set; }
    }
}
