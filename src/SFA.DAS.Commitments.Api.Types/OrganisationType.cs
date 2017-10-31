using System;

namespace SFA.DAS.Commitments.Api.Types
{
    [Obsolete("This enumeration has been superceeded by SFA.DAS.Common.Domain.Types.OrganisationType")]
    public enum OrganisationType : short
    {
        CompaniesHouse = 1,
        Charities = 2,
        PublicBodies = 3,
        Other = 4
    }
}