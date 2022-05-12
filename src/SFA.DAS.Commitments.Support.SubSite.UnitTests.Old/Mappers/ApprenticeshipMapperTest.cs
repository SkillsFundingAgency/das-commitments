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
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.HashingService;
using SFA.DAS.Commitments.Support.SubSite.Mappers;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Mappers
{
    [TestFixture]
    public class ApprenticeshipMapperTest
    {
        private Mock<IHashingService> _hashingService;
        private Apprenticeship _mockedApprenticeship;
        private Apprenticeship _mockedApprenticeshipNotConfirmedVersion;
        private Apprenticeship _mockedApprenticeshipNotConfirmedOption;
        private ApprenticeshipMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _hashingService = new Mock<IHashingService>();
            _mockedApprenticeship = new Apprenticeship
            {
                FirstName = "Test",
                LastName = "Me",
                TrainingCourseVersionConfirmed = true,
                TrainingCourseVersion = "1.1",
                TrainingCourseOption = "English",
                Email = "test@test.com",
                ConfirmationStatusDescription =  "Confirmed"
            };

            _mockedApprenticeshipNotConfirmedVersion = new Apprenticeship
            {
                FirstName = "Test",
                LastName = "Test2",
                TrainingCourseVersionConfirmed = false,
                TrainingCourseVersion = "1.1"
            };

            _mockedApprenticeshipNotConfirmedOption = new Apprenticeship
            {
                FirstName = "Test",
                LastName = "Test2",
                TrainingCourseVersionConfirmed = true,
                TrainingCourseVersion = "1.1",
                TrainingCourseOption = ""
            };

            _mapper = new ApprenticeshipMapper(_hashingService.Object);
        }

        [Test]
        public void ShouldMapToValidUlnSummaryViewModel()
        {
            var response = new GetApprenticeshipsByUlnResponse
            {
                TotalCount = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                   _mockedApprenticeship
                }
            };

            var result = _mapper.MapToUlnResultView(response);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSummaryViewModel>();
            result.ApprenticeshipsCount.Should().Be(1);
        }

        [Test]
        public void ShouldMapToValidApprenticeshipViewModel()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeship);
            result.Should().NotBeNull();
            result.Should().BeOfType<ApprenticeshipViewModel>();
        }

        [Test]
        public void ShouldMapApprenticeshipVersionToViewModelVersion()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeship);
            result.Version.Should().Be(_mockedApprenticeship.TrainingCourseVersion);
        }

        [Test]
        public void ShouldMapApprenticeshipOptionToViewModelOption()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeship);
            result.Option.Should().Be(_mockedApprenticeship.TrainingCourseOption);
        }

        public void ShouldMapApprenticeshipVersionNotConfirmedToViewModelVersionEmpty()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeshipNotConfirmedVersion);
            result.Version.Should().BeNullOrEmpty();
        }

        [Test]
        public void ShouldMapApprenticeshipNotYetConfirmedOptionToViewModelOptionToBeConfirmed()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeshipNotConfirmedOption);
            result.Option.Should().Be("To be confirmed");
        }

        [Test]
        public void ShouldMapApprenticeshipEmailToApprenticeshipViewModelEmail()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeship);
            result.Email.Should().Be(_mockedApprenticeship.Email);
        }

        [Test]
        public void ShouldMapConfirmationStatusToApprenticeshipViewModelConfirmationStatus()
        {
            var result = _mapper.MapToApprenticeshipViewModel(_mockedApprenticeship);
            result.ConfirmationStatusDescription.Should().Be(_mockedApprenticeship.ConfirmationStatusDescription);
        }

    }
}
