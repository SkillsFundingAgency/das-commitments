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
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ApiClientTests
{
    public class EmployerCommitmentApiTests
    {
        private FakeResponseHandler _fakeHandler;
        private EmployerCommitmentApi _apiclient;

        private const string ExpectedApiBaseUrl = "http://test.local.url/";
        private const long EmployerAccountId = 666;
        private const long ApprenticeshipId = 9990;

        [SetUp]
        public void Arrange()
        {
            _fakeHandler = new FakeResponseHandler();

            var httpClient = new HttpClient(_fakeHandler);

            var config = new Mock<ICommitmentsApiClientConfiguration>();
            config.Setup(m => m.BaseUrl).Returns(ExpectedApiBaseUrl);

            _apiclient = new EmployerCommitmentApi(httpClient, config.Object);
        }

        [Test]
        public async Task GettingProceHistory()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/prices"), string.Empty);
            var priceHistoryJson = JsonConvert.SerializeObject(
                new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 2000, FromDate = new DateTime(1998, 12, 8), ToDate = null },
                    new PriceHistory { ApprenticeshipId = ApprenticeshipId, Cost = 3000, FromDate = new DateTime(1882, 9, 5), ToDate = null }
                });
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var priceHistory = (await _apiclient.GetPriceHistory(EmployerAccountId, ApprenticeshipId)).ToArray();

            priceHistory.Length.Should().Be(2);
            priceHistory[0].Cost.Should().Be(2000);
            priceHistory[1].Cost.Should().Be(3000);
        }

        [Test]
        public async Task GetDataLockSummary()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/datalocksummary"), string.Empty);
            var priceHistoryJson = "{DataLockWithCourseMismatch:[],DataLockWithOnlyPriceMismatch:[]}";
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(priceHistoryJson) });

            var summary = await _apiclient.GetDataLockSummary(EmployerAccountId, ApprenticeshipId);

            summary.DataLockWithCourseMismatch.Count().Should().Be(0);
            summary.DataLockWithOnlyPriceMismatch.Count().Should().Be(0);
        }

        [Test]
        public async Task PatchDataLocks()
        {
            var submission = new DataLocksTriageResolutionSubmission
                                 {
                                     DataLockUpdateType = DataLockUpdateType.ApproveChanges,
                                     TriageStatus = TriageStatus.Change,
                                     UserId = "hello"
                                 };


            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/employer/{EmployerAccountId}/apprenticeships/{ApprenticeshipId}/datalocks/resolve"), JsonConvert.SerializeObject(submission));
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty) });

            await _apiclient.PatchDataLocks(EmployerAccountId, ApprenticeshipId, submission);

            Assert.Pass();
        }
    }
}
