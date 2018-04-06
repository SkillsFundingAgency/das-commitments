using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.HashingService;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class TransferRequestMapper : ITransferRequestMapper 
    {
        private readonly IHashingService _hashingService;

        public TransferRequestMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }
        public TransferRequestSummary MapFrom(Domain.Entities.TransferRequestSummary source)
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
                ApprovedOrRejectedOn = source.ApprovedOrRejectedOn
            };
        }

        public IEnumerable<TransferRequestSummary> MapFrom(IEnumerable<Domain.Entities.TransferRequestSummary> source)
        {
            return source.Select(MapFrom);
        }
    }
}