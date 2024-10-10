using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Models;

public enum OrganisationType : byte
{
    [Description("Listed on Companies House")] CompaniesHouse = 1,
    [Description("Charities")] Charities = 2,
    [Description("Public Bodies")] PublicBodies = 3,
    [Description("Other")] Other = 4,
}