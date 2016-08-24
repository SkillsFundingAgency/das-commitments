using System.Collections.Generic;
using SFA.DAS.ProviderApprenticeshipsService.Domain;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetUsers
{
    public class GetUsersQueryResponse
    {
        public List<User> Users { get; set; }
    }
}