using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Linq;
using static SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain.GetChangeOfProviderChainQueryResult;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Mappers
{
    [TestFixture]
    public class ApprenticeshipMapperTest
    {
        private Mock<IEncodingService> _encodingService;

        private SupportApprenticeshipDetails _mockedApprenticeship;
        private SupportApprenticeshipDetails _mockedApprenticeshipNotConfirmedVersion;
        private SupportApprenticeshipDetails _mockedApprenticeshipNotConfirmedOption;
        private GetSupportApprenticeshipQueryResult SupportApprenticeshipQueryResponse;
        private GetChangeOfProviderChainQueryResult ChangeOfProviderChainQueryResult;

        private ApprenticeshipMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _encodingService = new Mock<IEncodingService>();

            var dataFixture = new Fixture();
            _mockedApprenticeship = dataFixture.Build<SupportApprenticeshipDetails>().Create();
            _mockedApprenticeship.FirstName = "Test";
            _mockedApprenticeship.LastName = "Me";
            _mockedApprenticeship.TrainingCourseVersionConfirmed = true;
            _mockedApprenticeship.TrainingCourseVersion = "1.1";
            _mockedApprenticeship.TrainingCourseOption = "English";
            _mockedApprenticeship.Email = "test@test.com";
            _mockedApprenticeship.ConfirmationStatus = CommitmentsV2.Types.ConfirmationStatus.Confirmed;
            _mockedApprenticeship.AgreementStatus = CommitmentsV2.Types.AgreementStatus.BothAgreed;
            _mockedApprenticeship.PaymentStatus = CommitmentsV2.Types.PaymentStatus.Completed;

            _mockedApprenticeshipNotConfirmedVersion = dataFixture.Build<SupportApprenticeshipDetails>().Create();
            _mockedApprenticeshipNotConfirmedVersion.FirstName = "Test";
            _mockedApprenticeshipNotConfirmedVersion.LastName = "Test2";
            _mockedApprenticeshipNotConfirmedVersion.TrainingCourseVersionConfirmed = false;
            _mockedApprenticeshipNotConfirmedVersion.TrainingCourseVersion = "1.1";

            _mockedApprenticeshipNotConfirmedOption = dataFixture.Build<SupportApprenticeshipDetails>().Create();
            _mockedApprenticeshipNotConfirmedOption.FirstName = "Test";
            _mockedApprenticeshipNotConfirmedOption.LastName = "Test2";
            _mockedApprenticeshipNotConfirmedOption.TrainingCourseVersionConfirmed = true;
            _mockedApprenticeshipNotConfirmedOption.TrainingCourseVersion = "1.1";
            _mockedApprenticeshipNotConfirmedOption.TrainingCourseOption = "";

            SupportApprenticeshipQueryResponse = new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = new List<SupportApprenticeshipDetails>
                {
                    _mockedApprenticeship
                }
            };

            ChangeOfProviderChainQueryResult = dataFixture.Build<GetChangeOfProviderChainQueryResult>().Create();

            _mapper = new ApprenticeshipMapper(_encodingService.Object);
        }

        [Test]
        public void ShouldMapToValidUlnSummaryViewModel()
        {
            _encodingService
                .Setup(x => x.Encode(_mockedApprenticeship.EmployerAccountId, EncodingType.AccountId))
                .Returns("6PR88G");

            _encodingService
                .Setup(x => x.Encode(_mockedApprenticeship.Id, EncodingType.ApprenticeshipId))
                .Returns("V4G9RR");

            var result = _mapper.MapToUlnResultView(SupportApprenticeshipQueryResponse);

            _encodingService.Verify(x => x.Encode(_mockedApprenticeship.EmployerAccountId, EncodingType.AccountId), Times.AtLeastOnce);
            _encodingService.Verify(x => x.Encode(_mockedApprenticeship.Id, EncodingType.ApprenticeshipId), Times.AtLeastOnce);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSummaryViewModel>();
            result.ApprenticeshipsCount.Should().Be(1);

            result.SearchResults.Should().NotBeNullOrEmpty();

            result.SearchResults[0].ApprenticeshipHashId.Should().Be("V4G9RR");
            result.SearchResults[0].HashedAccountId.Should().Be("6PR88G");

            result.SearchResults[0].ApprenticeName.Should().Be($"{_mockedApprenticeship.FirstName} {_mockedApprenticeship.LastName}");
            result.SearchResults[0].ProviderUkprn.Should().Be(_mockedApprenticeship.ProviderId);
            result.SearchResults[0].EmployerName.Should().Be(_mockedApprenticeship.EmployerName);
            result.SearchResults[0].DateOfBirth.Should().Be(_mockedApprenticeship.DateOfBirth);
            result.SearchResults[0].Uln.Should().Be(_mockedApprenticeship.Uln);
        }

        [Test]
        public void ShouldMapToValidApprenticeshipViewModel()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Should().NotBeNull();
            result.Should().BeOfType<ApprenticeshipViewModel>();

            result.FirstName.Should().Be(_mockedApprenticeship.FirstName);
            result.LastName.Should().Be(_mockedApprenticeship.LastName);
            result.Email.Should().Be(_mockedApprenticeship.Email);
            result.ConfirmationStatusDescription.Should().Be("Confirmed");
            result.AgreementStatus.Should().Be("Both agreed");
            result.Uln.Should().Be(_mockedApprenticeship.Uln);
            result.DateOfBirth.Should().Be(_mockedApprenticeship.DateOfBirth);
            result.CohortReference.Should().Be(_mockedApprenticeship.CohortReference);
            result.EmployerReference.Should().Be(_mockedApprenticeship.EmployerRef);
            result.LegalEntity.Should().Be(_mockedApprenticeship.EmployerName);
            result.TrainingProvider.Should().Be(_mockedApprenticeship.ProviderName);
            result.UKPRN.Should().Be(_mockedApprenticeship.ProviderId);
            result.Trainingcourse.Should().Be(_mockedApprenticeship.CourseName);
            result.ApprenticeshipCode.Should().Be(_mockedApprenticeship.CourseCode);
            result.DasTrainingStartDate.Should().Be(_mockedApprenticeship.StartDate);
            result.DasTrainingEndDate.Should().Be(_mockedApprenticeship.EndDate);
            result.TrainingCost.Should().Be(_mockedApprenticeship.Cost);
            result.ApprenticeshipProviderHistory.Should().NotBeNullOrEmpty();
            result.ApprenticeshipProviderHistory.Count.Should().Be(ChangeOfProviderChainQueryResult.ChangeOfProviderChain.Count);
        }

        [TestCase(CommitmentsV2.Types.PaymentStatus.Active, "Live", "blue")]
        [TestCase(CommitmentsV2.Types.PaymentStatus.Paused, "Paused", "grey")]
        [TestCase(CommitmentsV2.Types.PaymentStatus.Withdrawn, "Stopped", "red")]
        [TestCase(CommitmentsV2.Types.PaymentStatus.Completed, "Completed", "green")]
        public void ShouldMapPaymentStatusCorrectly(CommitmentsV2.Types.PaymentStatus paymentStatus, string expectedPaymentStatusText, string expectedPaymentStatusTagColour)
        {
            SupportApprenticeshipQueryResponse.Apprenticeships.First().PaymentStatus = paymentStatus;
            SupportApprenticeshipQueryResponse.Apprenticeships.First().StartDate = System.DateTime.Now.AddMonths(-1);
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);

            result.PaymentStatus.Should().Be(expectedPaymentStatusText);
            result.PaymentStatusTagColour.Should().Be(expectedPaymentStatusTagColour);
        }

        [Test]
        public void ShouldMapApprenticeshipVersionToViewModelVersion()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Version.Should().Be(_mockedApprenticeship.TrainingCourseVersion);
        }

        [Test]
        public void ShouldMapApprenticeshipOptionToViewModelOption()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Option.Should().Be(_mockedApprenticeship.TrainingCourseOption);
        }

        [Test]
        public void ShouldMapApprenticeshipVersionNotConfirmedToViewModelVersionEmpty()
        {
            SupportApprenticeshipQueryResponse = new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = new List<SupportApprenticeshipDetails>
                {
                    _mockedApprenticeshipNotConfirmedVersion
                }
            };
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Version.Should().BeNullOrEmpty();
        }

        [Test]
        public void ShouldMapApprenticeshipNotYetConfirmedOptionToViewModelOptionToBeConfirmed()
        {
            SupportApprenticeshipQueryResponse = new GetSupportApprenticeshipQueryResult
            {
                Apprenticeships = new List<SupportApprenticeshipDetails>
                {
                    _mockedApprenticeshipNotConfirmedOption
                }
            };
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Option.Should().Be("To be confirmed");
        }

        [Test]
        public void ShouldMapApprenticeshipEmailToApprenticeshipViewModelEmail()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.Email.Should().Be(_mockedApprenticeship.Email);
        }

        [Test]
        public void ShouldMapConfirmationStatusToApprenticeshipViewModelConfirmationStatus()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.ConfirmationStatusDescription.Should().Be(_mockedApprenticeship.ConfirmationStatus.ToString());
        }

        [Test]
        public void ShouldMapApprenticeshipProviderHistory()
        {
            var changeOfProviderChain = new List<ChangeOfProviderLink>
            {
                new ChangeOfProviderLink
                {
                    ProviderName = "TEST 1",
                    StartDate = new System.DateTime(1,2,3)
                }
            };

            ChangeOfProviderChainQueryResult.ChangeOfProviderChain = changeOfProviderChain.AsReadOnly();

            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.ApprenticeshipProviderHistory.Should().NotBeNullOrEmpty();
            result.ApprenticeshipProviderHistory.Count.Should().Be(ChangeOfProviderChainQueryResult.ChangeOfProviderChain.Count);

            result.ApprenticeshipProviderHistory[0].ProviderName.Should().Be(changeOfProviderChain[0].ProviderName);
            result.ApprenticeshipProviderHistory[0].StartDate.Should().Be(changeOfProviderChain[0].StartDate);
        }

        [Test]
        public void WhenChangeOfProviderLinkIsNullShouldCreateNewInstanceForApprenticeshipProviderHistory()
        {
            ChangeOfProviderChainQueryResult.ChangeOfProviderChain = null;
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse, ChangeOfProviderChainQueryResult);
            result.ApprenticeshipProviderHistory.Should().NotBeNull();
        }
    }
}