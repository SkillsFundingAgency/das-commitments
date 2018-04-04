using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;
using TransferRequest = SFA.DAS.Commitments.Api.Types.Commitment.TransferRequest;
using TransferRequestSummary = SFA.DAS.Commitments.Api.Types.Commitment.TransferRequestSummary;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class TransferRequestMapper : ITransferRequestMapper 
    {
        public TransferRequestSummary MapFrom(Domain.Entities.TransferRequestSummary source)
        {
            return new TransferRequestSummary
            {
                TransferRequestId = source.TransferRequestId,

                ReceivingEmployerAccountId = source.ReceivingEmployerAccountId,
                CommitmentId = source.CommitmentId,
                SendingEmployerAccountId = source.SendingEmployerAccountId,
                TransferCost = source.TransferCost,
                Status = (TransferApprovalStatus) source.Status,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn
            };
        }

        public IEnumerable<TransferRequestSummary> MapFrom(IEnumerable<Domain.Entities.TransferRequestSummary> source)
        {
            return source.Select(MapFrom);
        }

        public TransferRequest MapFrom(Domain.Entities.TransferRequest source)
        {
            if (source == null)
                return null;

            return new TransferRequest
            {
                TransferRequestId = source.TransferRequestId,
                ReceivingEmployerAccountId = source.ReceivingEmployerAccountId,
                CommitmentId = source.CommitmentId,
                SendingEmployerAccountId = source.SendingEmployerAccountId,
                LegalEntityName = source.LegalEntityName,
                TransferCost = source.TransferCost,
                TrainingList = JsonConvert.DeserializeObject<List<Types.Commitment.TrainingCourseSummary>>(source.TrainingCourses),
                Status = (TransferApprovalStatus)source.Status,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn
            };
        }
    }
}