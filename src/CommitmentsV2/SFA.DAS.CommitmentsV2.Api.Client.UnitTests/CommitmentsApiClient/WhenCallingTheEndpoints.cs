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
        private WhenCallingTheEndpointsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenCallingTheEndpointsFixture(); 
        }

        [Test]
        public async Task GetLegalEntity_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetLegalEntity(123);
            _fixture.MockRestHttpClient.Verify(x=>x.Get<AccountLegalEntityResponse>("api/accountlegalentity/123", null, CancellationToken.None));
        }

        [Test]
        public async Task GetCohort_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetCohort(123);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetCohortResponse>("api/cohorts/123", null, CancellationToken.None));
        }

        [Test]
        public async Task GetDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetDraftApprenticeship(123, 456);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetDraftApprenticeshipResponse>("api/cohorts/123/draft-apprenticeships/456", null, CancellationToken.None));
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
        public async Task UpdateDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            const long cohortId = 67890;
            const long apprenticeshipId = 13456;
            await _fixture.CommitmentsApiClient.UpdateDraftApprenticeship(cohortId, apprenticeshipId, _fixture.UpdateDraftApprenticeshipRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PutAsJson($"api/cohorts/{cohortId}/draft-apprenticeships/{apprenticeshipId}", _fixture.UpdateDraftApprenticeshipRequest, CancellationToken.None));
        }
        
        [Test]
        public async Task AddDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.AddDraftApprenticeship(_fixture.CohortId, _fixture.AddDraftApprenticeshipRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<AddDraftApprenticeshipRequest>($"api/cohorts/{_fixture.CohortId}/draft-apprenticeships", _fixture.AddDraftApprenticeshipRequest, CancellationToken.None));
        }
    }

    public class WhenCallingTheEndpointsFixture
    {
        public Client.CommitmentsApiClient CommitmentsApiClient { get; }
        public Mock<IRestHttpClient> MockRestHttpClient { get; }
        public CreateCohortRequest CreateCohortRequest { get; }
        public UpdateDraftApprenticeshipRequest UpdateDraftApprenticeshipRequest { get; }
        public long CohortId { get; set; }
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }


        public WhenCallingTheEndpointsFixture()
        {
            MockRestHttpClient = new Mock<IRestHttpClient>();
            CommitmentsApiClient = new Client.CommitmentsApiClient(MockRestHttpClient.Object);
            CreateCohortRequest = new CreateCohortRequest();
            UpdateDraftApprenticeshipRequest = new UpdateDraftApprenticeshipRequest();
            CohortId = 123;
            AddDraftApprenticeshipRequest = new AddDraftApprenticeshipRequest();
        }

        public WhenCallingTheEndpointsFixture SetupResponseForCreateCohort()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson<CreateCohortRequest, CreateCohortResponse>(It.IsAny<string>(), It.IsAny<CreateCohortRequest>(), CancellationToken.None))
                .ReturnsAsync(new CreateCohortResponse());
            return this;
        }

        public WhenCallingTheEndpointsFixture SetupResponseForAddDraftApprenticeship()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson(It.IsAny<string>(), It.IsAny<AddDraftApprenticeshipRequest>(), CancellationToken.None))
                .ReturnsAsync("");
            return this;
        }
    }
}
