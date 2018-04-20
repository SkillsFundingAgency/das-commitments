using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver
{
    public sealed class GetTransferRequestsForReceiverResponse : QueryResponse<IList<TransferRequestSummary>>
    {
    }
}
