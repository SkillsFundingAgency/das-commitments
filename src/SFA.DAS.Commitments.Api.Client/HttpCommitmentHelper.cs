using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.Client
{
    internal class HttpCommitmentHelper : IHttpCommitmentHelper
    {
        private readonly SecureHttpClient _client;

        internal HttpCommitmentHelper(ICommitmentsApiClientConfiguration configuration)
        {
            _client = new SecureHttpClient(configuration);
        }

        internal HttpCommitmentHelper(ICommitmentsApiClientConfiguration configuration, SecureHttpClient client)
        {
            _client = client;
        }

        public async Task<CommitmentView> PostCommitment(string url, CommitmentRequest commitment)
        {
            var data = JsonConvert.SerializeObject(commitment);
            var content = await _client.PostAsync(url, data);

            return JsonConvert.DeserializeObject<CommitmentView>(content);
        }

        public async Task PatchCommitment(string url, CommitmentSubmission submision)
        {
            var data = JsonConvert.SerializeObject(submision);
            await _client.PatchAsync(url, data);
        }

        public async Task PutCommitment(string url, CommitmentStatus commitmentStatus)
        {
            var data = JsonConvert.SerializeObject(commitmentStatus);
            await _client.PutAsync(url, data);
        }

        public async Task PatchApprenticeship(string url, ApprenticeshipSubmission apprenticeshipSubmission)
        {
            var data = JsonConvert.SerializeObject(apprenticeshipSubmission);
            await _client.PatchAsync(url, data);
        }

        public async Task<List<CommitmentListItem>> GetCommitments(string url)
        {
            var content = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<List<CommitmentListItem>>(content);
        }

        public async Task<CommitmentView> GetCommitment(string url)
        {
            var content = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<CommitmentView>(content);
        }

        public async Task<List<Apprenticeship>> GetApprenticeships(string url)
        {
            var content = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<List<Apprenticeship>>(content);
        }

        public async Task<Apprenticeship> GetApprenticeship(string url)
        {
            var content = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<Apprenticeship>(content);
        }

        public async Task PutApprenticeship(string url, ApprenticeshipRequest apprenticeship)
        {
            var data = JsonConvert.SerializeObject(apprenticeship);
            await _client.PutAsync(url, data);
        }

        public async Task<Apprenticeship> PostApprenticeship(string url, ApprenticeshipRequest apprenticeship)
        {
            var data = JsonConvert.SerializeObject(apprenticeship);
            var content = await _client.PostAsync(url, data);

            return JsonConvert.DeserializeObject<Apprenticeship>(content);
        }

        public async Task<Apprenticeship> PostApprenticeships(string url, BulkApprenticeshipRequest bulkRequest)
        {
            var data = JsonConvert.SerializeObject(bulkRequest);
            var content = await _client.PostAsync(url, data);

            return JsonConvert.DeserializeObject<Apprenticeship>(content);
        }

        public async Task DeleteApprenticeship(string url, DeleteRequest deleteRequest)
        {
            var data = JsonConvert.SerializeObject(deleteRequest);
            await _client.DeleteAsync(url, data);
        }

        public async Task DeleteCommitment(string url, DeleteRequest deleteRequest)
        {
            var data = JsonConvert.SerializeObject(deleteRequest);
            await _client.DeleteAsync(url, data);
        }
        
        public async Task PostApprenticeshipUpdate(string url, ApprenticeshipUpdateRequest apprenticeshipUpdateRequest)
        {
            var data = JsonConvert.SerializeObject(apprenticeshipUpdateRequest);
            await _client.PostAsync(url, data);
        }

        public async Task<ApprenticeshipUpdate> GetApprenticeshipUpdate(string url)
        {
            var content = await _client.GetAsync(url);
            return JsonConvert.DeserializeObject<ApprenticeshipUpdate>(content);
        }

        public async Task PatchApprenticeshipUpdate(string url, ApprenticeshipUpdateSubmission submission)
        {
            var data = JsonConvert.SerializeObject(submission);
            await _client.PatchAsync(url, data);
        }
        
    }
}
