using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;

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
    }
}