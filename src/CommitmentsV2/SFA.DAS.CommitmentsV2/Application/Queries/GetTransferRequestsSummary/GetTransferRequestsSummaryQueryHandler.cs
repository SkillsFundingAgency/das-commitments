using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary
{
    public class GetTransferRequestsSummaryQueryHandler : IRequestHandler<GetTransferRequestsSummaryQuery, GetTransferRequestsSummaryQueryResult>
    {
        private readonly ITransferRequestDomainService _transferRequestDomainService;

        public GetTransferRequestsSummaryQueryHandler(ITransferRequestDomainService transferRequestDomainService)
        {
            if (transferRequestDomainService == null)
                throw new ArgumentNullException(nameof(transferRequestDomainService));
            _transferRequestDomainService = transferRequestDomainService;
        }

        public async Task<GetTransferRequestsSummaryQueryResult> Handle(GetTransferRequestsSummaryQuery message, CancellationToken cancellationToken)
        {
            return await _transferRequestDomainService.GetTransferRequestSummary(message.AccountId, message.Originator, cancellationToken);
        }
    }
}
