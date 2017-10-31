using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    [Obsolete("Please update your code to reference NuGet package SFA.DAS.Common.Domain and use its Types.OrganisationType enum")]
    public enum OrganisationType : short
    {
        CompaniesHouse = 1,
        Charities = 2,
        PublicBodies = 3,
        Other = 4
    }
}
