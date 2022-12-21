using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship
{
    public class OrganisationSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class EpaoResponse
    {
        public IEnumerable<OrganisationSummary> Epaos { get; set; }
    }
}
