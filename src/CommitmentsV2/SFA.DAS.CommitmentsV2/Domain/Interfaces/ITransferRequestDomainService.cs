using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ITransferRequestDomainService
    {
        Task ApproveTransferRequest(long transferRequestId, UserInfo userInfo, DateTime approvedOn, CancellationToken cancellationToken);
        Task RejectTransferRequest(long transferRequestId, UserInfo userInfo, DateTime rejectedOn, CancellationToken cancellationToken);
        Task<GetTransferRequestQueryResult> GetTransferRequest(long transferRequestId, long employerAccountId, CancellationToken cancellationToken);
        Task<List<EmployerTransferRequestPendingNotification>> GetEmployerTransferRequestPendingNotifications();
        Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestSummary(long accountId, TransferType? originator, CancellationToken cancellationToken);
    }
}