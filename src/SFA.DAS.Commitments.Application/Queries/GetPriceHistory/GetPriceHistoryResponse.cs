using System.Collections.Generic;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceHistory
{
    public class GetPriceHistoryResponse : QueryResponse<IEnumerable<PriceHistory>>
    {
    }
}