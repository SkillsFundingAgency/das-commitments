using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using AutoFixture;
using SFA.DAS.Encoding;

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

            _mapper = new ApprenticeshipMapper(_encodingService.Object);
        }

        [Test]
        public void ShouldMapToValidUlnSummaryViewModel()
        {
            var result = _mapper.MapToUlnResultView(SupportApprenticeshipQueryResponse);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSummaryViewModel>();
            result.ApprenticeshipsCount.Should().Be(1);
        }

        [Test]
        public void ShouldMapToValidApprenticeshipViewModel()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
            result.Should().NotBeNull();
            result.Should().BeOfType<ApprenticeshipViewModel>();
        }

        [Test]
        public void ShouldMapApprenticeshipVersionToViewModelVersion()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
            result.Version.Should().Be(_mockedApprenticeship.TrainingCourseVersion);
        }

        [Test]
        public void ShouldMapApprenticeshipOptionToViewModelOption()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
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
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
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
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
            result.Option.Should().Be("To be confirmed");
        }

        [Test]
        public void ShouldMapApprenticeshipEmailToApprenticeshipViewModelEmail()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
            result.Email.Should().Be(_mockedApprenticeship.Email);
        }

        [Test]
        public void ShouldMapConfirmationStatusToApprenticeshipViewModelConfirmationStatus()
        {
            var result = _mapper.MapToApprenticeshipViewModel(SupportApprenticeshipQueryResponse);
            result.ConfirmationStatusDescription.Should().Be(_mockedApprenticeship.ConfirmationStatus.ToString());
        }
    }
}