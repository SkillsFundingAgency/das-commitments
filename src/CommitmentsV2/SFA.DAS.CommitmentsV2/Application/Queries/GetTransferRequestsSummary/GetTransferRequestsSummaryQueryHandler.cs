using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;

public class GetTransferRequestsSummaryQueryHandler(ITransferRequestDomainService transferRequestDomainService) : IRequestHandler<GetTransferRequestsSummaryQuery, GetTransferRequestsSummaryQueryResult>
{
    public async Task<GetTransferRequestsSummaryQueryResult> Handle(GetTransferRequestsSummaryQuery message, CancellationToken cancellationToken)
    {
        return await transferRequestDomainService.GetTransferRequestSummary(message.AccountId, message.Originator, cancellationToken);
    }
}