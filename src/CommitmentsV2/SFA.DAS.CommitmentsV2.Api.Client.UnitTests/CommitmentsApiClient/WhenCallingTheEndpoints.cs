using System;
using System.Collections.Generic;
using System.Net;
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
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<AddDraftApprenticeshipRequest, AddDraftApprenticeshipResponse>($"api/cohorts/{_fixture.CohortId}/draft-apprenticeships", _fixture.AddDraftApprenticeshipRequest, CancellationToken.None));
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
            await _fixture.CommitmentsApiClient.GetAccountLegalEntity(123);
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

        [Test]
        public async Task GetApprenticeshipsForProvider_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
            };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>($"api/apprenticeships/?providerId={request.ProviderId}&reverseSort=False", null, CancellationToken.None));
        }

        
        [Test]
        public async Task GetApprenticeshipsForEmployer_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                AccountId = 10,
            };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>($"api/apprenticeships/?accountId={request.AccountId}&reverseSort=False", null, CancellationToken.None));
        }

         
        [Test]
        public void GetApprenticeships_WillThrowExceptionIfBothProviderIdAndEmployerAccountIdAreUsed()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 11,
                AccountId = 10,
            };

            //Act
            Assert.ThrowsAsync<NotSupportedException>(() => _fixture.CommitmentsApiClient.GetApprenticeships(request));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndPageNumberIsCorrectPassedIn()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                PageNumber = 2
            };
            

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?providerId={request.ProviderId}&reverseSort=False&pageNumber={request.PageNumber}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndPagItemCountIsCorrectPassedIn()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                PageItemCount = 3
            };
            
            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?providerId={request.ProviderId}&reverseSort=False&pageItemCount={request.PageItemCount}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndPageDataIsCorrectPassedIn()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                PageNumber = 2,
                PageItemCount = 3
            };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?providerId={request.ProviderId}&reverseSort=False&pageNumber={request.PageNumber}&pageItemCount={request.PageItemCount}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndDataIsCorrectPassedWithAdditionalFilter()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                SortField = "test"
            };
            
            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>($"api/apprenticeships/?providerId={request.ProviderId}&reverseSort=False&sortField={request.SortField}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndDataIsCorrectPassedWithSortAndReverse()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                SortField = "test",
                ReverseSort = true
            };
             
            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>($"api/apprenticeships/?providerId={request.ProviderId}&reverseSort={request.ReverseSort}&sortField={request.SortField}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeships_VerifyUrlAndDataIsCorrectWhenPassingFilterValues()
        {
            //Arrange
            var request = new GetApprenticeshipsRequest
            {
                ProviderId = 10,
                SearchTerm = "photon chocolate search termses",
                EmployerName = "Test corp",
                ProviderName = "Test Provider",
                CourseName = "Test course",
                Status = ApprenticeshipStatus.Completed,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddDays(4),
                AccountLegalEntityId = 123,
                StartDateRangeFrom = DateTime.Now.AddDays(-1),
                StartDateRangeTo = DateTime.Now.AddDays(1)
            };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeships(request);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsResponse>(
                $"api/apprenticeships/?" +
                $"providerId={request.ProviderId}&" +
                $"reverseSort={request.ReverseSort}&" +
                $"searchTerm={WebUtility.UrlEncode(request.SearchTerm)}&" +
                $"employerName={WebUtility.UrlEncode(request.EmployerName)}&" +
                $"providerName={WebUtility.UrlEncode(request.ProviderName)}&" +
                $"courseName={WebUtility.UrlEncode(request.CourseName)}&" +
                $"status={WebUtility.UrlEncode(request.Status.Value.ToString())}&" +
                $"startDate={WebUtility.UrlEncode(request.StartDate.Value.ToString("u"))}&" +
                $"endDate={WebUtility.UrlEncode(request.EndDate.Value.ToString("u"))}&" +
                $"accountLegalEntityId={request.AccountLegalEntityId.Value}&" +
                $"startDateRangeFrom={WebUtility.UrlEncode(request.StartDateRangeFrom.Value.ToString("u"))}&" +
                $"startDateRangeTo={WebUtility.UrlEncode(request.StartDateRangeTo.Value.ToString("u"))}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeshipFilterValues_VerifyUrlAndDataIsCorrectForProvider()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest{ ProviderId = 10 };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeshipsFilterValues(request, CancellationToken.None);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsFilterValuesResponse>($"api/apprenticeships/filters?providerId={request.ProviderId}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeshipFilterValues_VerifyUrlAndDataIsCorrectForEmployer()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest { EmployerAccountId = 10 };

            //Act
            await _fixture.CommitmentsApiClient.GetApprenticeshipsFilterValues(request, CancellationToken.None);

            //Assert$
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipsFilterValuesResponse>($"api/apprenticeships/filters?employerAccountId={request.EmployerAccountId}", null, CancellationToken.None));
        }

        [Test]
        public void GetApprenticeshipFilterValues_VerifyExceptionIsThrowIfBothProviderAndEmployerIdIsUsed()
        {
            //Arrange
            var request = new GetApprenticeshipFiltersRequest
            {
                ProviderId = 12,
                EmployerAccountId = 10
            };

            //Act
            Assert.ThrowsAsync<NotSupportedException>(() =>_fixture.CommitmentsApiClient.GetApprenticeshipsFilterValues(request, CancellationToken.None));
        }

        [Test]
        public async Task GetAccount_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetAccount(123, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.Get<AccountResponse>("api/accounts/123", null, CancellationToken.None));
        }

        [Test]
        public async Task GetProviderPaymentPriority_VerifyUrlAndDataIsCorrectPassedIn()
        {
            await _fixture.CommitmentsApiClient.GetProviderPaymentsPriority(123, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetProviderPaymentsPriorityResponse>("api/accounts/123/provider-payments-priority", null, CancellationToken.None));
        }

        [Test]
        public async Task UpdateProviderPaymentPriority_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var accountId = 123;
            var request = new UpdateProviderPaymentsPriorityRequest
            {
                ProviderPriorities = new List<UpdateProviderPaymentsPriorityRequest.ProviderPaymentPriorityUpdateItem>(),
                UserInfo = new UserInfo()
            };
            
            //Act
            await _fixture.CommitmentsApiClient.UpdateProviderPaymentsPriority(accountId, request, CancellationToken.None);
            
            // Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/accounts/{accountId}/update-provider-payments-priority", request, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeship_VerifyUrlAndData()
        {
            await _fixture.CommitmentsApiClient.GetApprenticeship(123);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipResponse>("api/apprenticeships/123", null, CancellationToken.None));
        }

        [Test]
        public async Task GetPriceEpisodes_VerifyUrlAndData()
        {
            await _fixture.CommitmentsApiClient.GetPriceEpisodes(123);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetPriceEpisodesResponse>("api/apprenticeships/123/price-episodes", null, CancellationToken.None));
        }

        [TestCase(1, ApprenticeshipUpdateStatus.Approved)]
        [TestCase(2, ApprenticeshipUpdateStatus.Deleted)]
        [TestCase(3, ApprenticeshipUpdateStatus.Pending)]
        [TestCase(4, ApprenticeshipUpdateStatus.Rejected)]
        public async Task GetApprenticeshipUpdates_WithStatus_VerifyUrlAndData(int apprenticeshipId, ApprenticeshipUpdateStatus status)
        {
            var request = new GetApprenticeshipUpdatesRequest { Status = status };
            await _fixture.CommitmentsApiClient.GetApprenticeshipUpdates(apprenticeshipId, request);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipUpdatesResponse>($"api/apprenticeships/{apprenticeshipId}/updates?status={status}", null, CancellationToken.None));
        }

        [Test]
        public async Task GetApprenticeshipUpdates_WithNullStatus_VerifyUrlAndData()
        {
            var request = new GetApprenticeshipUpdatesRequest();
            await _fixture.CommitmentsApiClient.GetApprenticeshipUpdates(1, request);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetApprenticeshipUpdatesResponse>($"api/apprenticeships/{1}/updates", null, CancellationToken.None));
        }

        [Test]
        public async Task GetChangeOfPartyRequests_VerifyUrlAndData()
        {
            await _fixture.CommitmentsApiClient.GetChangeOfPartyRequests(123);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetChangeOfPartyRequestsResponse>("api/apprenticeships/123/change-of-party-requests", null, CancellationToken.None));
        }

        [Test]
        public async Task GetChangeOfProviderChain_VerifyUrlAndData()
        {
            await _fixture.CommitmentsApiClient.GetChangeOfProviderChain(12345, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetChangeOfProviderChainResponse>("api/apprenticeships/12345/change-of-provider-chain", null, CancellationToken.None));
        }

        [Test]
        public async Task GetChangeOfEmployerChain_VerifyUrlAndData()
        {
            await _fixture.CommitmentsApiClient.GetChangeOfEmployerChain(12345, CancellationToken.None);
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetChangeOfEmployerChainResponse>("api/apprenticeships/12345/change-of-employer-chain", null, CancellationToken.None));
        }

        [Test]
        public async Task StopApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var apprenticeshipId = 11;
            var request = new StopApprenticeshipRequest
            {
                AccountId = 10,
                StopDate = DateTime.UtcNow,
                MadeRedundant = true,
                UserInfo = new UserInfo()
            };

            //Act
            await _fixture.CommitmentsApiClient.StopApprenticeship(apprenticeshipId, request, CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/{apprenticeshipId}/stop", request, CancellationToken.None));
        }

        [Test]
        public async Task ResumeApprenticeship_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var apprenticeshipId = 11;
            var request = new ResumeApprenticeshipRequest
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = new UserInfo()
            };

            //Act
            await _fixture.CommitmentsApiClient.ResumeApprenticeship(request, CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/details/resume", request, CancellationToken.None));
        }

        [Test]
        public async Task PauseApprenticeship_VerifyUrlAndData()
        {
            var request = new PauseApprenticeshipRequest
            {
                ApprenticeshipId = 11
            };

            await _fixture.CommitmentsApiClient.PauseApprenticeship(request, CancellationToken.None);

            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/details/pause", request, CancellationToken.None));
        }

        [Test]
        public async Task GetAllStandards()
        {
            await _fixture.CommitmentsApiClient.GetAllTrainingProgrammeStandards();
            
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetAllTrainingProgrammeStandardsResponse>("api/TrainingProgramme/standards", null, CancellationToken.None));
        }

        [Test]
        public async Task GetEmailOverlapChecks()
        {
            await _fixture.CommitmentsApiClient.GetEmailOverlapChecks(123);

            _fixture.MockRestHttpClient.Verify(x => x.Get<GetEmailOverlapsResponse>("api/cohorts/123/email-overlaps", null, CancellationToken.None));
        }

        [Test]
        public async Task GetAllTrainingProgrammes()
        {
            await _fixture.CommitmentsApiClient.GetAllTrainingProgrammes();
                
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetAllTrainingProgrammesResponse>("api/TrainingProgramme/all", null, CancellationToken.None));
        }

        [Test]
        public async Task GetTrainingProgrammeById()
        {
            await _fixture.CommitmentsApiClient.GetTrainingProgramme("123");
            
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetTrainingProgrammeResponse>("api/TrainingProgramme/123", null, CancellationToken.None));
        }

        [Test]
        public async Task GetTrainingProgrammeVersions()
        {
            await _fixture.CommitmentsApiClient.GetTrainingProgrammeVersions("123");

            _fixture.MockRestHttpClient.Verify(x => x.Get<GetTrainingProgrammeVersionsResponse>("api/TrainingProgramme/123/versions", null, CancellationToken.None));
        }

        [Test]
        public async Task EditApprenticeship_VerifyUrl()
        {
            var request = new EditApprenticeshipApiRequest();
            await _fixture.CommitmentsApiClient.EditApprenticeship(request);
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson<EditApprenticeshipApiRequest, EditApprenticeshipResponse>("api/apprenticeships/edit", request, CancellationToken.None));
        }

        [Test]
        public async Task AcceptApprenticeshipUpdates_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var apprenticeshipId = 11;
            var request = new AcceptApprenticeshipUpdatesRequest
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = new UserInfo()
            };

            //Act
            await _fixture.CommitmentsApiClient.AcceptApprenticeshipUpdates(apprenticeshipId, request, CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/accept-apprenticeship-update", request, CancellationToken.None));
        }

        [Test]
        public async Task RejectApprenticeshipUpdates_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var apprenticeshipId = 11;
            var request = new RejectApprenticeshipUpdatesRequest
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = new UserInfo()
            };

            //Act
            await _fixture.CommitmentsApiClient.RejectApprenticeshipUpdates(apprenticeshipId, request, CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/reject-apprenticeship-update", request, CancellationToken.None));
        }

        [Test]
        public async Task UndoApprenticeshipUpdates_VerifyUrlAndDataIsCorrectPassedIn()
        {
            //Arrange
            var apprenticeshipId = 11;
            var request = new UndoApprenticeshipUpdatesRequest
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = new UserInfo()
            };

            //Act
            await _fixture.CommitmentsApiClient.UndoApprenticeshipUpdates(apprenticeshipId, request, CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.PostAsJson($"api/apprenticeships/{apprenticeshipId}/updates/undo-apprenticeship-update", request, CancellationToken.None));
        }

        [Test]
        public async Task GetProviderCommitmentAgreement_VerifyUrlAndDataIsCorrectlyPassedIn()
        {
            //Arrange
            var providerId = 11;

            //Act
            await _fixture.CommitmentsApiClient.GetProviderCommitmentAgreement(providerId,  CancellationToken.None);

            //Assert
            _fixture.MockRestHttpClient.Verify(x => x.Get<GetProviderCommitmentAgreementResponse>($"api/providers/{ providerId}/commitmentagreements", null, CancellationToken.None));
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
