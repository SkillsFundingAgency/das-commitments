using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.HashingService;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;
using TransferRequest = SFA.DAS.Commitments.Api.Types.Commitment.TransferRequest;
using TransferRequestSummary = SFA.DAS.Commitments.Api.Types.Commitment.TransferRequestSummary;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class TransferRequestMapper : ITransferRequestMapper 
    {
        private readonly IHashingService _hashingService;

        public TransferRequestMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }
        public TransferRequestSummary MapFrom(Domain.Entities.TransferRequestSummary source, TransferType transferType)
        {
            return new TransferRequestSummary
            {
                HashedTransferRequestId = _hashingService.HashValue(source.TransferRequestId),
                HashedReceivingEmployerAccountId = _hashingService.HashValue(source.ReceivingEmployerAccountId),
                HashedCohortRef = _hashingService.HashValue(source.CommitmentId),
                HashedSendingEmployerAccountId = _hashingService.HashValue(source.SendingEmployerAccountId),
                TransferCost = source.TransferCost,
                Status = (TransferApprovalStatus) source.Status,
                ApprovedOrRejectedByUserName = source.ApprovedOrRejectedByUserName,
                ApprovedOrRejectedByUserEmail = source.ApprovedOrRejectedByUserEmail,
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn,
                CreatedOn = source.CreatedOn,
                TransferType = transferType
            };
        }

        public IEnumerable<TransferRequestSummary> MapFrom(IEnumerable<Domain.Entities.TransferRequestSummary> source, TransferType transferType)
        {
            return source.Select(x=>MapFrom(x, transferType));
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