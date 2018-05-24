using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Commitment;
using TransferRequestSummary = SFA.DAS.Commitments.Api.Types.Commitment.TransferRequestSummary;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface ITransferRequestMapper
    {
        TransferRequestSummary MapFrom(Domain.Entities.TransferRequestSummary source, TransferType transferType);
        IEnumerable<TransferRequestSummary> MapFrom(IEnumerable<Domain.Entities.TransferRequestSummary> source, TransferType transferType);
        TransferRequest MapFrom(Domain.Entities.TransferRequest source);
    }
}
