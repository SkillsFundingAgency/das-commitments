using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Client
{
    public class RelationshipApi : IRelationshipApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;
        private readonly SecureHttpClient _client;
        private string BaseUrl => _configuration.BaseUrl.EndsWith("/") ? _configuration.BaseUrl : _configuration.BaseUrl + "/";

        public RelationshipApi(ICommitmentsApiClientConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
            _client = new SecureHttpClient(configuration);

        }

        internal RelationshipApi(ICommitmentsApiClientConfiguration configuration, SecureHttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<Relationship> GetRelationship(long providerId, long employerAccountId, string legalEntityId)
        {
            var url = $"{BaseUrl}api/provider/{providerId}/relationships/{employerAccountId}/{legalEntityId}";
            return await GetRelationship(url);
        }

        public async Task PatchRelationship(long providerId, long employerAccountId, string legalEntityId, RelationshipRequest relationshipRequest)
        {
            var url = $"{BaseUrl}api/provider/{providerId}/relationships/{employerAccountId}/{legalEntityId}";
            await PatchRelationship(url, relationshipRequest);
        }

        public async Task<Relationship> GetRelationshipByCommitment(long providerId, long commitmentId)
        {
            var url = $"{BaseUrl}api/provider/{providerId}/relationships/{commitmentId}";
            return await GetRelationship(url);
        }

        private async Task<Relationship> GetRelationship(string url)
        {
            var content = await _client.GetAsync(url);
            return JsonConvert.DeserializeObject<Relationship>(content);
        }

        private async Task PatchRelationship(string url, RelationshipRequest relationshipRequest)
        {
            var data = JsonConvert.SerializeObject(relationshipRequest);
            await _client.PatchAsync(url, data);
        }
    }
}