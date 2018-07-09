using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IAssessmentOrgs
    {
        Task<IEnumerable<OrganisationSummary>> All();
    }
}
