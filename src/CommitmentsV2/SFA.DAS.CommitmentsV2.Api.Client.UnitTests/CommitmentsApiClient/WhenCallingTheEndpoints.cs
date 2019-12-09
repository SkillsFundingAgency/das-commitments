using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;
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
        public async Task Ping_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.Ping();
            _fixture.MockRestHttpClient.Verify(x => x.Get("api/ping", null, CancellationToken.None));
        }

        [Test]
        public async Task WhoAmI_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.WhoAmI();
            _fixture.MockRestHttpClient.Verify(x => x.Get<WhoAmIResponse>("api/whoami", null, CancellationToken.None));
        }
        
        [Test]
        public async Task AddDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.AddDraftApprenticeship(_fixture.CohortId, _fixture.AddDraftApprenticeshipRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<AddDraftApprenticeshipRequest>($"api/cohorts/{_fixture.CohortId}/draft-apprenticeships", _fixture.AddDraftApprenticeshipRequest, CancellationToken.None));
        }

        [Test]
        public async Task Approve_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.ApproveCohort(123, _fixture.ApproveCohortRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.PostAsJson("api/cohorts/123/approve", _fixture.ApproveCohortRequest, CancellationToken.None));
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
        public async Task GetCohorts_VerifyUrlAndDataIsCorrectPassedIn()
        {
            var request = new GetCohortsRequest {AccountId = 123};
            await _fixture.CommitmentsApiClient.GetCohorts(request);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetCohortsResponse>("api/cohorts", request, CancellationToken.None));
        }

        [Test]
        public async Task GetDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetDraftApprenticeship(123, 456);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetDraftApprenticeshipResponse>("api/cohorts/123/draft-apprenticeships/456", null, CancellationToken.None));
        }

        [Test]
        public async Task GetDraftApprenticeships_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetDraftApprenticeships(123);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetDraftApprenticeshipsResponse>("api/cohorts/123/draft-apprenticeships", null, CancellationToken.None));
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
        public async Task CreateCohortWithOtherParty_VerifyUrlAndDataIsCorrectPassedIn()
        {

            await _fixture.CommitmentsApiClient.CreateCohort(_fixture.CreateCohortWithOtherPartyRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<CreateCohortWithOtherPartyRequest, CreateCohortResponse>("api/cohorts/create-with-other-party", _fixture.CreateCohortWithOtherPartyRequest, CancellationToken.None));
        }

        [Test]
        public async Task CreateCohortWithOtherParty_VerifyResponseWasReturned()
        {
            _fixture.SetupResponseForCreateCohortWithOtherParty();
            var result = await _fixture.CommitmentsApiClient.CreateCohort(_fixture.CreateCohortWithOtherPartyRequest, CancellationToken.None);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetProvider_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetProvider(123);
            _fixture.MockRestHttpClient.Verify(c => c.Get<GetProviderResponse>("api/providers/123", null, CancellationToken.None));
        }

        [Test]
        public async Task Send_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.SendCohort(123, _fixture.SendCohortRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.PostAsJson("api/cohorts/123/send", _fixture.SendCohortRequest, CancellationToken.None));
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
        public async Task GetLatestAgreementId_VerifyUrlAndDataIsCorrectlyPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetLatestAgreementId(123, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.Get<long?>("api/employer-agreements/123/latest-id", null, CancellationToken.None));
        }

        [Test]
        public async Task IsAgreementSigned_When_NoFeatureSpecified_VerifyNoUrlDataIsPassedIn()
        {
            var request = new AgreementSignedRequest
            {
                AccountLegalEntityId = 123,
                AgreementFeatures = null
            };

            await _fixture.CommitmentsApiClient.IsAgreementSigned(request, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.Get<bool>("api/employer-agreements/123/signed", null, CancellationToken.None));
        }

        [Test]
        public async Task IsAgreementSigned_When_EmptyArrayOfFeaturesSpecified_VerifyNoUrlDataIsPassedIn()
        {
            var request = new AgreementSignedRequest
            {
                AccountLegalEntityId = 123,
                AgreementFeatures = new AgreementFeature[0]
            };

            await _fixture.CommitmentsApiClient.IsAgreementSigned(request, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.Get<bool>("api/employer-agreements/123/signed", null, CancellationToken.None));
        }

        [Test]
        public async Task IsAgreementSigned_When_OneFeaturesSpecified_VerifyNoUrlDataIsPassedIn()
        {
            var request = new AgreementSignedRequest
            {
                AccountLegalEntityId = 123,
                AgreementFeatures = new AgreementFeature[] { AgreementFeature.Transfers }
            };

            await _fixture.CommitmentsApiClient.IsAgreementSigned(request, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.Get<bool>("api/employer-agreements/123/signed?agreementFeatures=Transfers", null, CancellationToken.None));
        }

        [Test]
        public async Task DeleteDraftApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            const long cohortId = 67890;
            const long apprenticeshipId = 13456;
            await _fixture.CommitmentsApiClient.DeleteDraftApprenticeship(cohortId, apprenticeshipId, _fixture.DeleteDraftApprenticeshipRequest, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.PostAsJson($"api/cohorts/{cohortId}/draft-apprenticeships/{apprenticeshipId}", _fixture.DeleteDraftApprenticeshipRequest, CancellationToken.None));
        }

        [Test]
        public async Task DeleteCohort_VerifyUrlAndDataIsCorrectPassedIn()
        {
            const long cohortId = 67890;
            await _fixture.CommitmentsApiClient.DeleteCohort(cohortId, _fixture.UserInfo, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(c => c.PostAsJson($"api/cohorts/{cohortId}/delete", _fixture.UserInfo, CancellationToken.None));
        }
    }

    public class WhenCallingTheEndpointsFixture
    {
        public Client.CommitmentsApiClient CommitmentsApiClient { get; }
        public Mock<IRestHttpClient> MockRestHttpClient { get; }
        public AddDraftApprenticeshipRequest AddDraftApprenticeshipRequest { get; set; }
        public ApproveCohortRequest ApproveCohortRequest { get; }
        public CreateCohortRequest CreateCohortRequest { get; }
        public CreateCohortWithOtherPartyRequest CreateCohortWithOtherPartyRequest { get; }
        public SendCohortRequest SendCohortRequest { get; }
        public UpdateDraftApprenticeshipRequest UpdateDraftApprenticeshipRequest { get; }
        public DeleteDraftApprenticeshipRequest DeleteDraftApprenticeshipRequest { get; }
        public UserInfo UserInfo { get; }
        public long CohortId { get; set; }
        
        public WhenCallingTheEndpointsFixture()
        {
            MockRestHttpClient = new Mock<IRestHttpClient>();
            CommitmentsApiClient = new Client.CommitmentsApiClient(MockRestHttpClient.Object);
            AddDraftApprenticeshipRequest = new AddDraftApprenticeshipRequest();
            ApproveCohortRequest = new ApproveCohortRequest();
            CreateCohortRequest = new CreateCohortRequest();
            CreateCohortWithOtherPartyRequest = new CreateCohortWithOtherPartyRequest();
            SendCohortRequest = new SendCohortRequest();
            UpdateDraftApprenticeshipRequest = new UpdateDraftApprenticeshipRequest();
            DeleteDraftApprenticeshipRequest = new DeleteDraftApprenticeshipRequest();
            UserInfo = new UserInfo();
            CohortId = 123;
        }

        public WhenCallingTheEndpointsFixture SetupResponseForCreateCohort()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson<CreateCohortRequest, CreateCohortResponse>(It.IsAny<string>(), It.IsAny<CreateCohortRequest>(), CancellationToken.None))
                .ReturnsAsync(new CreateCohortResponse());
            return this;
        }

        public WhenCallingTheEndpointsFixture SetupResponseForCreateCohortWithOtherParty()
        {
            MockRestHttpClient.Setup(x => x.PostAsJson<CreateCohortWithOtherPartyRequest, CreateCohortResponse>(It.IsAny<string>(), It.IsAny<CreateCohortWithOtherPartyRequest>(), CancellationToken.None))
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
