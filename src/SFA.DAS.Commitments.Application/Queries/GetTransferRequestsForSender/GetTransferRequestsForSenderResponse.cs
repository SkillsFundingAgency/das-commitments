using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender
{
    public sealed class GetTransferRequestsForSenderResponse : QueryResponse<IList<TransferRequestSummary>>
    {
    }
}
