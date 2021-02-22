using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;

namespace SFA.DAS.Commitments.Api.Client.UnitTests.ApiClientTests
{
    public class TrainingProgrammeApiTests
    {
        private FakeResponseHandler _fakeHandler;
        private TrainingProgrammeApi _trainingProgrammeApi;
        private const string ExpectedApiBaseUrl = "http://test.local.url/";
        
        [SetUp]
        public void Arrange()
        {
            _fakeHandler = new FakeResponseHandler();

            var httpClient = new HttpClient(_fakeHandler);

            var config = new Mock<ICommitmentsApiClientConfiguration>();
            config.Setup(m => m.BaseUrl).Returns(ExpectedApiBaseUrl);

            _trainingProgrammeApi = new TrainingProgrammeApi(httpClient, config.Object);
        }
        
        [Test]
        public async Task GetTrainingProgramme()
        {
            var trainingCode = "123-ab";
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/trainingprogramme/{trainingCode}"), string.Empty);
            var getProviderResponse = new GetTrainingProgrammeResponse
            {
                TrainingProgramme = new TrainingProgramme()
            };
            var content = JsonConvert.SerializeObject(
                getProviderResponse);
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) });

            var response = (await _trainingProgrammeApi.Get(trainingCode));

            response.ShouldBeEquivalentTo(getProviderResponse);
        }
        
        
        [Test]
        public async Task GetAllTrainingProgrammes()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/trainingprogramme/all"), string.Empty);
            var response = new GetAllTrainingProgrammesResponse
            {
                TrainingProgrammes = new List<TrainingProgramme>()
            };
            var content = JsonConvert.SerializeObject(response);
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) });

            var actual = (await _trainingProgrammeApi.GetAll());

            actual.ShouldBeEquivalentTo(response);
        }
        
        [Test]
        public async Task GetAllStandardsTrainingProgrammes()
        {
            var request = new TestRequest(new Uri(ExpectedApiBaseUrl + $"api/trainingprogramme/standards"), string.Empty);
            var response = new GetAllTrainingProgrammeStandardsResponse
            {
                TrainingProgrammes = new List<TrainingProgramme>()
            };
            var content = JsonConvert.SerializeObject(response);
            _fakeHandler.AddFakeResponse(request, new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) });

            var actual = (await _trainingProgrammeApi.GetAllStandards());

            actual.ShouldBeEquivalentTo(response);
        }
    }
}