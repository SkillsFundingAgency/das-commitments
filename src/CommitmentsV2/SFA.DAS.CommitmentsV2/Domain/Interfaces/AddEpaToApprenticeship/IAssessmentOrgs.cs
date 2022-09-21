using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship
{
    internal interface IAssessmentOrgs
    {
        Task<IEnumerable<OrganisationSummary>> All();
    }
}
