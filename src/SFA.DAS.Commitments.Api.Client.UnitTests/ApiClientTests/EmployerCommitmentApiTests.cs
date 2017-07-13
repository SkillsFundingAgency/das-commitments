﻿using System;
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
using SFA.DAS.Commitments.Api.Types.ProviderPayment;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ApiClientTests
{
    [TestFixture]
    public class EmployerCommitmentApiTests
    {
        private FakeResponseHandler _fakeHandler;
        private EmployerCommitmentApi _employerApiClient;

        private const string ExpectedApiBaseUrl = "http://test.local.url/";
        private const long EmployerAccountId = 666;
        private const long ApprenticeshipId = 9990;
        private const long CommitmentId = 876;

        [SetUp]
        public void Arrange()
        {
            _fakeHandler = new FakeResponseHandler();

            var httpClient = new HttpClient(_fakeHandler);

            var config = new Mock<ICommitmentsApiClientConfiguration>();
            config.Setup(m => m.BaseUrl).Returns(ExpectedApiBaseUrl);

            _employerApiClient = new EmployerCommitmentApi(httpClient, config.Object);
        }

        [Test]
        public async Task GetEmployerAccountSummary()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            var accountSummaryList = await _employerApiClient.GetEmployerAccountSummary(EmployerAccountId);

            Assert.Pass();
        }

        [Test]
        public async Task CreateEmployerCommitment()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments"), JsonConvert.SerializeObject(new CommitmentRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            var commitmentView = await _employerApiClient.CreateEmployerCommitment(EmployerAccountId, new CommitmentRequest());

            Assert.Pass();
        }

        [Test]
        public async Task GetEmployerCommitments()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            var commitmentList = await _employerApiClient.GetEmployerCommitments(EmployerAccountId);

            Assert.Pass();
        }

        [Test]
        public async Task GetEmployerCommitment()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments/{CommitmentId}"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new CommitmentView())) });

            var commitmentView = await _employerApiClient.GetEmployerCommitment(EmployerAccountId, CommitmentId);

            Assert.Pass();
        }

        [Test]
        public async Task GetEmployerApprenticeships()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new List<Apprenticeship>())) });

            var apprenticeship = await _employerApiClient.GetEmployerApprenticeships(EmployerAccountId);

            Assert.Pass();
        }

        [Test]
        public async Task GetEmployerApprenticeship()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new Apprenticeship())) });

            var apprenticeship = await _employerApiClient.GetEmployerApprenticeship(EmployerAccountId, ApprenticeshipId);

            Assert.Pass();
        }

        [Test]
        public async Task PostCommitment()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments/{CommitmentId}"), JsonConvert.SerializeObject(new CommitmentSubmission()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.PatchEmployerCommitment(EmployerAccountId, CommitmentId, new CommitmentSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task CreateEmployerApprenticeship()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments/{CommitmentId}/apprenticeships"), JsonConvert.SerializeObject(new ApprenticeshipRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.CreateEmployerApprenticeship(EmployerAccountId, CommitmentId, new ApprenticeshipRequest());

            Assert.Pass();
        }

        [Test]
        public async Task UpdateEmployerApprenticeship()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments/{CommitmentId}/apprenticeships/{ApprenticeshipId}"), JsonConvert.SerializeObject(new ApprenticeshipRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.UpdateEmployerApprenticeship(EmployerAccountId, CommitmentId, ApprenticeshipId, new ApprenticeshipRequest() );

            Assert.Pass();
        }

        [Test]
        public async Task PatchEmployerApprenticeship()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}"), JsonConvert.SerializeObject(new ApprenticeshipSubmission()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.PatchEmployerApprenticeship(EmployerAccountId, ApprenticeshipId, new ApprenticeshipSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task DeleteEmployerApprenticeship()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}"), JsonConvert.SerializeObject(new DeleteRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.DeleteEmployerApprenticeship(EmployerAccountId, ApprenticeshipId, new DeleteRequest());

            Assert.Pass();
        }

        [Test]
        public async Task DeleteEmployerCommitment()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/commitments/{CommitmentId}"), JsonConvert.SerializeObject(new DeleteRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.DeleteEmployerCommitment(EmployerAccountId, CommitmentId, new DeleteRequest());

            Assert.Pass();
        }

        [Test]
        public async Task CreateApprenticeshipUpdate()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/update"), JsonConvert.SerializeObject(new ApprenticeshipUpdateRequest()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new Apprenticeship())) });

            await _employerApiClient.CreateApprenticeshipUpdate(EmployerAccountId, ApprenticeshipId, new ApprenticeshipUpdateRequest());

            Assert.Pass();
        }

        [Test]
        public async Task GetPendingApprenticeshipUpdate()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/update"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new ApprenticeshipUpdate())) });

            var apprenticeship = await _employerApiClient.GetPendingApprenticeshipUpdate(EmployerAccountId, ApprenticeshipId);

            Assert.Pass();
        }

        [Test]
        public async Task PatchApprenticeshipUpdate()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/update"), JsonConvert.SerializeObject(new ApprenticeshipUpdateSubmission()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.PatchApprenticeshipUpdate(EmployerAccountId, ApprenticeshipId, new ApprenticeshipUpdateSubmission());

            Assert.Pass();
        }

        [Test]
        public async Task GetCustomProviderPaymentPriority()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/customproviderpaymentpriority/"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new List<ProviderPaymentPriorityItem>())) });

            var apprenticeship = await _employerApiClient.GetCustomProviderPaymentPriority(EmployerAccountId);

            Assert.Pass();
        }

        [Test]
        public async Task UpdateCustomProviderPaymentPriority()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/customproviderpaymentpriority/"), JsonConvert.SerializeObject(new ProviderPaymentPrioritySubmission()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.UpdateCustomProviderPaymentPriority(EmployerAccountId, new ProviderPaymentPrioritySubmission());

            Assert.Pass();
        }


        [Test]
        public async Task GetPriceHistory()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/prices"), string.Empty);
            var priceHistoryJson = JsonConvert.SerializeObject(
                new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 2000, FromDate = new DateTime(1998, 12, 8), ToDate = null },
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 3000, FromDate = new DateTime(1882, 9, 5), ToDate = null }
                });
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var priceHistory = (await _employerApiClient.GetPriceHistory(EmployerAccountId, ApprenticeshipId)).ToArray();

            priceHistory.Length.Should().Be(2);
            priceHistory[0].Cost.Should().Be(2000);
            priceHistory[1].Cost.Should().Be(3000);
        }

        [Test]
        public async Task GetDataLocks()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/datalocks"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new List<DataLockStatus>())) });

            var apprenticeship = await _employerApiClient.GetDataLocks(EmployerAccountId, ApprenticeshipId);

            Assert.Pass();
        }
        
        [Test]
        public async Task GetDataLockSummary()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/datalocksummary"), string.Empty);
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(new DataLockSummary())) });

            var apprenticeship = await _employerApiClient.GetDataLockSummary(EmployerAccountId, ApprenticeshipId);

            Assert.Pass();
        }

        [Test]
        public async Task PatchDataLocks()
        {
            var employerRequest = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/datalocks/resolve"), JsonConvert.SerializeObject(new DataLocksTriageResolutionSubmission()));
            _fakeHandler.AddFakeResponse(employerRequest, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _employerApiClient.PatchDataLocks(EmployerAccountId, ApprenticeshipId, new DataLocksTriageResolutionSubmission());

            Assert.Pass();
        }
    }
}
