using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.AzureStorage;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class AssessmentOrgsDocumentService : IAssessmentOrgs
    {
        private readonly IAzureBlobStorage _azureBlobStorage;

        public AssessmentOrgsDocumentService(IAzureBlobStorage azureBlobStorage)
        {
            _azureBlobStorage = azureBlobStorage;
        }

        public async Task<IEnumerable<OrganisationSummary>> All()
        {
            const string containerName = "assessmentorgs-repository";
            const string blobName = "assessment_orgs.json";

            var result = await _azureBlobStorage.ReadBlob(containerName, blobName);
            if (string.IsNullOrEmpty(result))
                return new OrganisationSummary[0];

            return JsonConvert.DeserializeObject<IEnumerable<OrganisationSummary>>(result);
        }
    }
}
