using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Authentication.Extensions.Legacy;

namespace SFA.DAS.Commitments.Api.Client
{
    public class EmployerCommitmentApi : ApiClientBase, IEmployerCommitmentApi
    {
        private readonly ICommitmentsApiClientConfiguration _configuration;

        private readonly IHttpCommitmentHelper _commitmentHelper;

        public EmployerCommitmentApi(HttpClient client, ICommitmentsApiClientConfiguration configuration)
            : base(client)
        {
            if(configuration == null)
                throw new ArgumentException(nameof(configuration));
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _configuration = configuration;
            _commitmentHelper = new HttpCommitmentHelper(client);
        }

        public async Task<List<ApprenticeshipStatusSummary>> GetEmployerAccountSummary(long employerAccountId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/";

            return await _commitmentHelper.GetEmployerAccountSummary(url);
        }

        public async Task<List<CommitmentListItem>> GetEmployerCommitments(long employerAccountId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/commitments";

            return await _commitmentHelper.GetCommitments(url);
        }

        public async Task<CommitmentView> GetEmployerCommitment(long employerAccountId, long commitmentId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            return await _commitmentHelper.GetCommitment(url);
        }

        public async Task<CommitmentView> GetTransferSenderCommitment(long transferSenderAccountId, long commitmentId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{transferSenderAccountId}/transfers/{commitmentId}";

            return await _commitmentHelper.GetCommitment(url);
        }

        public async Task<List<Apprenticeship>> GetEmployerApprenticeships(long employerAccountId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/";

            return await _commitmentHelper.GetApprenticeships(url);
        }

        public async Task<ApprenticeshipSearchResponse> GetEmployerApprenticeships(long employerAccountId, ApprenticeshipSearchQuery apprenticeshipSearchQuery)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/search";

            return await _commitmentHelper.GetApprenticeships(url, apprenticeshipSearchQuery);
        }

        public async Task<Apprenticeship> GetEmployerApprenticeship(long employerAccountId, long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}";

            return await _commitmentHelper.GetApprenticeship(url);
        }

        public async Task PatchEmployerCommitment(long employerAccountId, long commitmentId, CommitmentSubmission submission)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            await _commitmentHelper.PatchCommitment(url, submission);
        }

        public async Task<IList<Apprenticeship>> GetActiveApprenticeshipsForUln(long employerAccountId, string uln)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/uln/{uln}";

            return await _commitmentHelper.GetApprenticeships(url);
        }

        public async Task PatchEmployerApprenticeship(long employerAccountId, long apprenticeshipId, ApprenticeshipSubmission apprenticeshipSubmission)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}";

            await _commitmentHelper.PatchApprenticeship(url, apprenticeshipSubmission);
        }

        public async Task DeleteEmployerApprenticeship(long employerAccountId, long apprenticeshipId, DeleteRequest deleteRequest)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}";

            await _commitmentHelper.DeleteApprenticeship(url, deleteRequest);
        }

        public async Task DeleteEmployerCommitment(long employerAccountId, long commitmentId, DeleteRequest deleteRequest)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}";

            await _commitmentHelper.DeleteCommitment(url, deleteRequest);
        }

        public async Task CreateApprenticeshipUpdate(long employerAccountId, long apprenticeshipId, ApprenticeshipUpdateRequest apprenticeshipUpdateRequest)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/update";

            await _commitmentHelper.PostApprenticeshipUpdate(url, apprenticeshipUpdateRequest);
        }

        public async Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long employerAccountId, long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/update";

            return await _commitmentHelper.GetApprenticeshipUpdate(url);
        }

        public async Task PatchApprenticeshipUpdate(long employerAccountId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/update";

            await _commitmentHelper.PatchApprenticeshipUpdate(url, submission);
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistory(long employerAccountId, long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/prices";
            var content = await GetAsync(url);
            return JsonConvert.DeserializeObject<IEnumerable<PriceHistory>>(content);
        }

        public async Task<List<DataLockStatus>> GetDataLocks(long employerAccountId, long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/datalocks";
            var content = await GetAsync(url);
            return JsonConvert.DeserializeObject<List<DataLockStatus>>(content);
        }

        public async Task<DataLockSummary> GetDataLockSummary(long employerAccountId, long apprenticeshipId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/datalocksummary";
            var content = await GetAsync(url);
            return JsonConvert.DeserializeObject<DataLockSummary>(content);
        }

        public async Task PatchDataLocks(long employerAccountId, long apprenticeshipId, DataLocksTriageResolutionSubmission submission)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}/datalocks/resolve";
            var data = JsonConvert.SerializeObject(submission);
            await PatchAsync(url, data);
        }

        public async Task PutApprenticeshipStopDate(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipStopDate stopDate)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}/stopdate";
            var data = JsonConvert.SerializeObject(stopDate);
            await PutAsync(url, data);
        }

        public async Task ApproveCohort(long employerAccountId, long commitmentId, CommitmentSubmission submission)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{employerAccountId}/commitments/{commitmentId}/approve";

            await _commitmentHelper.PatchCommitment(url, submission);
        }

        public Task PatchTransferApprovalStatus(long transferSenderId, long commitmentId, long transferRequestId, TransferApprovalRequest request)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{transferSenderId}/transfers/{transferRequestId}/approval/{commitmentId}";
            var data = JsonConvert.SerializeObject(request);
            return PatchAsync(url, data);
        }

        public Task<List<TransferRequestSummary>> GetTransferRequests(string hashedAccountId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{hashedAccountId}/transfers";
            return _commitmentHelper.GetTransferRequests(url);
        }

        public Task<TransferRequest> GetTransferRequestForSender(long transferSenderId, long transferRequestId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{transferSenderId}/sender/transfers/{transferRequestId}";
            return _commitmentHelper.GetTransferRequest(url);
        }
        public Task<TransferRequest> GetTransferRequestForReceiver(long transferSenderId, long transferRequestId)
        {
            var url = $"{_configuration.BaseUrl}api/employer/{transferSenderId}/receiver/transfers/{transferRequestId}";
            return _commitmentHelper.GetTransferRequest(url);
        }

        public Task<IEnumerable<long>> GetAllEmployerAccountIds()
        {
            var url = $"{_configuration.BaseUrl}api/employer/ids";
            return _commitmentHelper.GetUrlResult<IEnumerable<long>>(url);
        }
    }
}