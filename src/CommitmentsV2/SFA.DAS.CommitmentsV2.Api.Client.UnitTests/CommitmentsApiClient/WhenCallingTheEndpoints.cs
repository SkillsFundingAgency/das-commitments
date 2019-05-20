using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.UnitTests.CommitmentsApiClient
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingTheEndpoints
    {
        private WhenCallingEndpointsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenCallingEndpointsFixture(); 
        }

        [Test]
        public async Task GetLegalEntity_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetLegalEntity(123);
            _fixture.MockRestHttpClient.Verify(x=>x.Get<AccountLegalEntityResponse>("api/accountlegalentity/123", null, CancellationToken.None));
        }

        [Test]
        public async Task CreateCohort_VerifyUrlAndDataIsCorrectPassedIn()
        {

            await _fixture.CommitmentsApiClient.CreateCohort(_fixture.CreateCohortRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<CreateCohortRequest, CreateCohortResponse>("api/cohorts", _fixture.CreateCohortRequest, CancellationToken.None));
        }
        
        [Test]
        public async Task CreateCohort_VerifyResponseWasReturned()
        {
            _fixture.SetupResponseForCreateCohort();
            var result = await _fixture.CommitmentsApiClient.CreateCohort(_fixture.CreateCohortRequest, CancellationToken.None);
            Assert.IsNotNull(result);
        }
        
        [Test]
        public async Task AddDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.AddDraftApprenticeship(_fixture.AddDraftApprenticeshipRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<AddDraftApprenticeshipRequest>($"api/cohorts/{_fixture.AddDraftApprenticeshipRequest.CohortId}/draft-apprenticeships", _fixture.AddDraftApprenticeshipRequest, CancellationToken.None));
        }
    }

    public class WhenCallingEndpointsFixture
    {
        public Client.CommitmentsApiClient CommitmentsApiClient;
        public Mock<IRestHttpClient> MockRestHttpClient;
        public CreateCohortRequest CreateCohortRequest;
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }

        public WhenCallingEndpointsFixture()
        {
            MockRestHttpClient = new Mock<IRestHttpClient>();
            CommitmentsApiClient = new Client.CommitmentsApiClient(MockRestHttpClient.Object);
            CreateCohortRequest = new CreateCohortRequest();
            AddDraftApprenticeshipRequest = new AddDraftApprenticeshipRequest { CohortId = 123 };
        }

        public WhenCallingEndpointsFixture SetupResponseForCreateCohort()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson<CreateCohortRequest, CreateCohortResponse>(It.IsAny<string>(), It.IsAny<CreateCohortRequest>(), CancellationToken.None))
                .ReturnsAsync(new CreateCohortResponse());
            return this;
        }

        public WhenCallingEndpointsFixture SetupResponseForAddDraftApprenticeship()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson(It.IsAny<string>(), It.IsAny<AddDraftApprenticeshipRequest>(), CancellationToken.None))
                .ReturnsAsync("");
            return this;
        }
    }
}
