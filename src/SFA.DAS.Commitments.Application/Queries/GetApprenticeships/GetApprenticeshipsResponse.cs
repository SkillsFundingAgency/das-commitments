using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeships
{
    //todo: return list of summary type instead?
    public sealed class GetApprenticeshipsResponse : QueryResponse<IList<Apprenticeship>>
    {
    }
}
