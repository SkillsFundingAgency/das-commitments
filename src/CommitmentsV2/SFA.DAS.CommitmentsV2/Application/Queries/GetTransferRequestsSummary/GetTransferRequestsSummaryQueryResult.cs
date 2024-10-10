using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;

public class GetTransferRequestsSummaryQueryResult
{
    public IEnumerable<TransferRequestsSummaryQueryResult> TransferRequestsSummaryQueryResult { get; set; }
}   

public class TransferRequestsSummaryQueryResult
{
    public long TransferRequestId { get; set; }
    public long ReceivingEmployerAccountId { get; set; }
    public long CommitmentId { get; set; }
    public string CohortReference { get; set; }
    public long SendingEmployerAccountId { get; set; }
    public decimal TransferCost { get; set; }
    public TransferApprovalStatus Status { get; set; }
    public string ApprovedOrRejectedByUserName { get; set; }
    public string ApprovedOrRejectedByUserEmail { get; set; }
    public DateTime? ApprovedOrRejectedOn { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime CreatedOn { get; set; }
    public int FundingCap { get; set; }
}