using System.Collections.Generic;

using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetCustomProviderPaymentsPriority
{
    public sealed class GetProviderPaymentsPriorityResponse : QueryResponse<IEnumerable<ProviderPaymentPriorityItem>>
    {
    }
}
