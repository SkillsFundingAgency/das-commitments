using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

public class GetTransferRequestsSummaryResponseMapper(IEncodingService encodingService) : IMapper<GetTransferRequestsSummaryQueryResult, GetTransferRequestSummaryResponse>
{
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
            HashedReceivingEmployerAccountId = encodingService.Encode(source.ReceivingEmployerAccountId, EncodingType.AccountId),
            HashedSendingEmployerAccountId = encodingService.Encode(source.SendingEmployerAccountId, EncodingType.AccountId),
            HashedTransferRequestId = encodingService.Encode(source.TransferRequestId, EncodingType.AccountId),
            Status = source.Status,
            TransferCost = source.TransferCost,
            TransferType = source.TransferType
        };
    }
}