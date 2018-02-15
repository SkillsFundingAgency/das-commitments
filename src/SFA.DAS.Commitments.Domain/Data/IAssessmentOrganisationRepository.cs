using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IAssessmentOrganisationRepository
    {
        Task<string> GetLatestEpaOrgId();

        Task Add(IEnumerable<AssessmentOrganisation> assessmentOrganisations);
    }
}
