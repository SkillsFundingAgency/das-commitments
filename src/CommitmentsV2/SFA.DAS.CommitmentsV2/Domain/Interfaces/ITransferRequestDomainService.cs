using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ITransferRequestDomainService
    {
        Task ApproveTransferRequest(long transferRequestId, UserInfo userInfo, DateTime approvedOn, CancellationToken cancellation);
        Task RejectTransferRequest(long transferRequestId, UserInfo userInfo, DateTime rejectedOn, CancellationToken cancellationToken);
        Task<GetTransferRequestQueryResult> GetTransferRequest(long transferRequestId, long employerAccountId, CancellationToken cancellationToken);
        Task<GetTransferRequestsSummaryQueryResult> GetTransferRequestSummary(long accountId, CancellationToken cancellationToken);
    }
}