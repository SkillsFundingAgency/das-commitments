using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Api.Types;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IAssessmentOrgs
    {
        Task<IEnumerable<OrganisationSummary>> All();
    }
}
