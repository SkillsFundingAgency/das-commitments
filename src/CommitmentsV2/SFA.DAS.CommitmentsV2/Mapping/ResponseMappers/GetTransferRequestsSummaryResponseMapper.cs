using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetTransferRequestsSummaryResponseMapper : IMapper<GetTransferRequestsSummaryQueryResult, GetTransferRequestSummaryResponse>
    {
        private readonly IEncodingService _encodingService;
        public GetTransferRequestsSummaryResponseMapper(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }
        
        public Task<GetTransferRequestSummaryResponse> Map(GetTransferRequestsSummaryQueryResult source)
        {
            return Task.FromResult(new GetTransferRequestSummaryResponse
            {
                TransferRequestSummaryResponse = source.TransferRequestsSummaryQueryResult.Select(MapApprenticeship)
            });
        }

        private TransferRequestSummaryResponse MapApprenticeship(TransferRequestsSummaryQueryResult source)
        {
            return new TransferRequestSummaryResponse
            {
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn,
                CreatedOn = source.CreatedOn,
                FundingCap = source.FundingCap,
                CommitmentId = source.CommitmentId,
                CohortReference = source.CohortReference,
                HashedReceivingEmployerAccountId = _encodingService.Encode(source.ReceivingEmployerAccountId, EncodingType.AccountId),
                HashedSendingEmployerAccountId = _encodingService.Encode(source.SendingEmployerAccountId, EncodingType.AccountId),
                HashedTransferRequestId = _encodingService.Encode(source.TransferRequestId, EncodingType.AccountId),
                Status = (TransferApprovalStatus)source.Status,
                TransferCost = source.TransferCost,
                TransferType = source.TransferType
            };
        }
    }
}
