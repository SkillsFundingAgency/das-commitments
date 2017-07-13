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
    [TestFixture]
    public class ProviderCommitmentApiTests
    {
        private ProviderCommitmentsApi _apiclient;
        private FakeResponseHandler _fakeHandler;

        private const string ExpectedApiBaseUrl = "http://test.local.url/";
        private const long ProviderId = 444;
        private const long ApprenticeshipId = 9990;

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
    }
}
