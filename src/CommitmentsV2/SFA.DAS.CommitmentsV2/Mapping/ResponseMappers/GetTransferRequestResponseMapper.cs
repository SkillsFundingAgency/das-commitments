using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetTransferRequestResponseMapper : IMapper<GetTransferRequestQueryResult, GetTransferRequestResponse>
    {
        public Task<GetTransferRequestResponse> Map(GetTransferRequestQueryResult source)
        {
            return Task.FromResult(new GetTransferRequestResponse
            {
                TransferRequestId = source.TransferRequestId,
                ReceivingEmployerAccountId = source.ReceivingEmployerAccountId,
                CommitmentId = source.CommitmentId,
                SendingEmployerAccountId = source.SendingEmployerAccountId,
                TransferSenderName = source.TransferSenderName,
                LegalEntityName = source.LegalEntityName,
                TransferCost = source.TransferCost,
                TrainingList = JsonConvert.DeserializeObject<List<TrainingCourseSummary>>(source.TrainingCourses),
                Status = source.Status,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn,
                FundingCap = source.FundingCap,
                AutoApproval = source.AutoApproval,
                PledgeApplicationId = source.PledgeApplicationId
            });
        }
    }
}