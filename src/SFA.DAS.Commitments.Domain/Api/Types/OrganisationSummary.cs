using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Api.Types
{
    public class OrganisationSummary
    {
        public string Id { get ; set ; }
        public string Name { get ; set ; }
    }

    public class EpaoResponse
    {
        public IEnumerable<OrganisationSummary> Epaos { get; set; }
    }
}