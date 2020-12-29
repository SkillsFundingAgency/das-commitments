using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ApiClientTests
{
    [TestFixture]
    public class ProviderCommitmentApiTests
    {
        private ProviderCommitmentsApi _apiclient;
        private FakeResponseHandler _fakeHandler;

        private const string ExpectedApiBaseUrl = "http://test.local.url/";
        private const long ProviderId = 444;
        private const long ApprenticeshipId = 9990;
        private const long CommitmentId = 876;

        [SetUp]
        public void Arrange()
        {
            _fakeHandler = new FakeResponseHandler();

            var httpClient = new HttpClient(_fakeHandler);

            var config = new Mock<ICommitmentsApiClientConfiguration>();
            config.Setup(m => m.BaseUrl).Returns(ExpectedApiBaseUrl);

            _apiclient = new ProviderCommitmentsApi(httpClient, config.Object);
        }

        [Test]
        public async Task GetPriceHistory()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/prices"), string.Empty);
            var priceHistoryJson = JsonConvert.SerializeObject(
                new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 2000, FromDate = new DateTime(1998, 12, 8), ToDate = null },
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 3000, FromDate = new DateTime(1882, 9, 5), ToDate = null }
                });
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var priceHistory = (await  _apiclient.GetPriceHistory(ProviderId, ApprenticeshipId)).ToArray();

            priceHistory.Length.Should().Be(2);
            priceHistory[0].Cost.Should().Be(2000);
            priceHistory[1].Cost.Should().Be(3000);
        }

        [Test]
        public async Task GetDataLocks()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/datalocks"), string.Empty);
            var priceHistoryJson = JsonConvert.SerializeObject(
                new List<DataLockStatus>
                {
                    new DataLockStatus { ApprenticeshipId = ApprenticeshipId, ErrorCode = DataLockErrorCode.Dlock02 },
                    new DataLockStatus { ApprenticeshipId = ApprenticeshipId, ErrorCode = DataLockErrorCode.Dlock10 }
                });
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var dataLocks = (await _apiclient.GetDataLocks(ProviderId, ApprenticeshipId)).ToArray();
            dataLocks.Length.Should().Be(2);
            dataLocks[0].ErrorCode.Should().HaveFlag(DataLockErrorCode.Dlock02);
            dataLocks[0].ErrorCode.Should().NotHaveFlag(DataLockErrorCode.Dlock10);
            dataLocks[1].ErrorCode.Should().HaveFlag(DataLockErrorCode.Dlock10);
            dataLocks[1].ErrorCode.Should().NotHaveFlag(DataLockErrorCode.Dlock02);
        }

        [Test]
        public async Task GetDataLockSummary()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/datalocksummary"), string.Empty);
            var priceHistoryJson = "{DataLockWithCourseMismatch:[],DataLockWithOnlyPriceMismatch:[]}";
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var summary = await _apiclient.GetDataLockSummary(ProviderId, ApprenticeshipId);

            summary.DataLockWithCourseMismatch.Count().Should().Be(0);
            summary.DataLockWithOnlyPriceMismatch.Count().Should().Be(0);
        }

        [Test]
        public async Task PatchDataLock()
        {
            var submission = new DataLockTriageSubmission
            {
                TriageStatus = TriageStatus.Change,
                UserId = "hello champ"
            };
            var dataLockId = 007;

            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/datalocks/{dataLockId}"), JsonConvert.SerializeObject(submission));
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.PatchDataLock(ProviderId, ApprenticeshipId, dataLockId, submission);

            Assert.Pass();
        }

        [Test]
        public async Task PatchDataLocks()
        {
            var submission = new DataLockTriageSubmission
            {
                TriageStatus = TriageStatus.Change,
                UserId = "hello"
            };


            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/datalocks"), JsonConvert.SerializeObject(submission));
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.PatchDataLocks(ProviderId, ApprenticeshipId, submission);

            Assert.Pass();
        }

        [Test]
        public async Task BulkUploadApprenticeships()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments/{CommitmentId}/apprenticeships/bulk"), JsonConvert.SerializeObject(new BulkApprenticeshipRequest()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.BulkUploadApprenticeships(ProviderId, CommitmentId, new BulkApprenticeshipRequest());
        }

        [Test]
        public async Task BulkUploadFilePost()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/bulkupload"), JsonConvert.SerializeObject(new BulkUploadFileRequest()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("1") });

            await _apiclient.BulkUploadFile(ProviderId, new BulkUploadFileRequest());
        }

        [Test]
        public async Task BulkUploadFile()
        {
            var bulkUploadFileId = 1007;
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/bulkupload/{bulkUploadFileId}"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("") });

            await _apiclient.BulkUploadFile(ProviderId, bulkUploadFileId);
        }


        [Test]
        public async Task GetProviderCommitments()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            var commitmentList = await _apiclient.GetProviderCommitments(ProviderId);

            Assert.Pass();
        }

        [Test]
        public async Task GetCommitmentAgreements()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitmentagreements"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.GetCommitmentAgreements(ProviderId);

            Assert.Pass();
        }

        [Test]
        public async Task GetProviderCommitment()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments/{CommitmentId}"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new CommitmentView())) });

            var commitmentView = await _apiclient.GetProviderCommitment(ProviderId, CommitmentId);

            Assert.Pass();
        }

        [Test]
        public async Task GetProviderApprenticeships()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new List<Apprenticeship>())) });

            var apprenticeship = await _apiclient.GetProviderApprenticeships(ProviderId);

            Assert.Pass();
        }

        [Test]
        public async Task GetProviderApprenticeship()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new Apprenticeship())) });

            var apprenticeship = await _apiclient.GetProviderApprenticeship(ProviderId, ApprenticeshipId);

            Assert.Pass();
        }

        [Test]
        public async Task PostCommitment()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments/{CommitmentId}"), JsonConvert.SerializeObject(new CommitmentSubmission()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.PatchProviderCommitment(ProviderId, CommitmentId, new CommitmentSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task DeleteProviderApprenticeship()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}"), JsonConvert.SerializeObject(new DeleteRequest()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.DeleteProviderApprenticeship(ProviderId, ApprenticeshipId, new DeleteRequest());

            Assert.Pass();
        }

        [Test]
        public async Task DeleteProviderCommitment()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments/{CommitmentId}"), JsonConvert.SerializeObject(new DeleteRequest()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.DeleteProviderCommitment(ProviderId, CommitmentId, new DeleteRequest());

            Assert.Pass();
        }

        [Test]
        public async Task CreateApprenticeshipUpdate()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/update"), JsonConvert.SerializeObject(new ApprenticeshipUpdateRequest()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new Apprenticeship())) });

            await _apiclient.CreateApprenticeshipUpdate(ProviderId, ApprenticeshipId, new ApprenticeshipUpdateRequest());

            Assert.Pass();
        }

        [Test]
        public async Task GetPendingApprenticeshipUpdate()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/update"), string.Empty);
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new ApprenticeshipUpdate())) });

            var apprenticeship = await _apiclient.GetPendingApprenticeshipUpdate(ProviderId, ApprenticeshipId);

            Assert.Pass();
        }

        [Test]
        public async Task PatchApprenticeshipUpdate()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/apprenticeships/{ApprenticeshipId}/update"), JsonConvert.SerializeObject(new ApprenticeshipUpdateSubmission()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.PatchApprenticeshipUpdate(ProviderId, ApprenticeshipId, new ApprenticeshipUpdateSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task ApproveCohort()
        {
            var providerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}/commitments/{CommitmentId}/approve"), JsonConvert.SerializeObject(new CommitmentSubmission()));
            _fakeHandler.AddFakeResponse(providerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.ApproveCohort(ProviderId, CommitmentId, new CommitmentSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task GetProvider()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/provider/{ProviderId}"), string.Empty);
            var getProviderResponse = new GetProviderResponse
            {
                Provider = new ProviderResponse()
            };
            var content = JsonConvert.SerializeObject(
                getProviderResponse);
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) });

            var response = (await _apiclient.GetProvider(ProviderId));

            response.ShouldBeEquivalentTo(getProviderResponse);
        }
    }
}
