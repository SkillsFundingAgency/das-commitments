using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class AuthorizationControllerTests
    {
        private AuthorizationControllerTestFixture _fixture;

        [SetUp]
        public void Initialise()
        {
            _fixture = new AuthorizationControllerTestFixture();
        }

        [TestCase(Party.Provider, 124, 1)]
        [TestCase(Party.Employer, 123, 2)]
        public async Task AuthorizationController_AccessCohortRequest_ShouldCallCommandWithCorrectQueryValues(
            Party party, long partyId, long cohortId)
        {
            var request = new CohortAccessRequest {CohortId = cohortId, Party = party, PartyId = partyId};

            await _fixture.AuthorizationController.CanAccessCohort(request);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<CanAccessCohortQuery>(p => p.CohortId == cohortId &&
                p.Party == party &&
                p.PartyId == partyId), 
                CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task AuthorizationController_AccessCohortRequest_ShouldReturnOkAndBoolResult()
        {
            var request = new CohortAccessRequest { CohortId = 1, Party = Party.Employer, PartyId = 2 };

            var retVal = await _fixture.SetCanAccessCohortToReturnTrue().AuthorizationController.CanAccessCohort(request);

            Assert.IsInstanceOf<OkObjectResult>(retVal);
            Assert.IsTrue((bool)((OkObjectResult)retVal).Value);
        }

        [TestCase(Party.Provider, 124, 1)]
        [TestCase(Party.Employer, 123, 2)]
        public async Task AuthorizationController_AccessApprenticeshipRequest_ShouldCallCommandWithCorrectQueryValues(
            Party party, long partyId, long apprenticeshipId)
        {
            var request = new ApprenticeshipAccessRequest { ApprenticeshipId = apprenticeshipId, Party = party, PartyId = partyId };

            await _fixture.AuthorizationController.CanAccessApprenticeship(request);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<CanAccessApprenticeshipQuery>(p => p.ApprenticeshipId == apprenticeshipId &&
                p.Party == party &&
                p.PartyId == partyId), 
                CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task AuthorizationController_AccessApprenticeshipRequest_ShouldReturnOkAndBoolResult()
        {
            var request = new ApprenticeshipAccessRequest { ApprenticeshipId = 1, Party = Party.Employer, PartyId = 2 };

            var retVal = await _fixture.SetCanAccessApprenticeshipToReturnTrue().AuthorizationController.CanAccessApprenticeship(request);

            Assert.IsInstanceOf<OkObjectResult>(retVal);
            Assert.IsTrue((bool)((OkObjectResult)retVal).Value);
        }

        [Test]
        public void AuthorizationController_ApprenticeEmailRequired_ShouldReturnOk()
        {
            var providerId = 123456;
            
            var retVal = _fixture
                .SetApprenticeEmailFeatureOnOff(true)
                .SetApprenticeEmailRequiredForProviderToReturnTrue(providerId).AuthorizationController.ApprenticeEmailRequired(providerId);

            Assert.IsInstanceOf<OkResult>(retVal);
        }

        [Test]
        public void AuthorizationController_ApprenticeEmailRequired_ShouldReturnNotFound()
        {
            var providerId = 123456;

            var retVal = _fixture.AuthorizationController.ApprenticeEmailRequired(providerId);

            Assert.IsInstanceOf<NotFoundResult>(retVal);
        }

        [Test]
        public void AuthorizationController_WhenFeatureIsOff_ApprenticeEmailRequired_ShouldReturnNotFound()
        {
            var providerId = 123456;

            _fixture.SetApprenticeEmailFeatureOnOff(false);
            _fixture.SetApprenticeEmailRequiredForProviderToReturnTrue(providerId);

            var retVal = _fixture.AuthorizationController.ApprenticeEmailRequired(providerId);

            Assert.IsInstanceOf<NotFoundResult>(retVal);
        }

        [TestCase(123, 321)]
        [TestCase(456, 654)]
        public async Task AuthorizationController_email_optional_sends_correct_query(long employerId, long providerId)
        {
            await _fixture.AuthorizationController.OptionalEmail(employerId, providerId);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<GetEmailOptionalQuery>(q => 
                q.EmployerId == employerId && q.ProviderId == providerId), CancellationToken.None), Times.Once);
        }

        [TestCase(123, 321)]
        [TestCase(456, 0)]
        [TestCase(0, 987)]
        public void AuthorizationController_email_optional_test_handler_positive(long employerId, long providerId)
        {
            var sut = new GetEmailOptionalQueryHandler(new EmailOptionalConfiguration { EmailOptionalEmployers = new long[] { 123, 456, 789 }, EmailOptionalProviders = new long[] { 321, 654, 987 } });
            var query = new GetEmailOptionalQuery(employerId, providerId);

            var result = sut.Handle(query, new CancellationToken()).Result;

            Assert.IsTrue(result);
        }

        [TestCase(78901, 10)]
        [TestCase(0, 456)]
        [TestCase(987, 0)]
        public void AuthorizationController_email_optional_test_handler_negative(long employerId, long providerId)
        {
            var sut = new GetEmailOptionalQueryHandler(new EmailOptionalConfiguration { EmailOptionalEmployers = new long[] { 123, 456, 789 }, EmailOptionalProviders = new long[] { 321, 654, 987 } });
            var query = new GetEmailOptionalQuery(employerId, providerId);

            var result = sut.Handle(query, new CancellationToken()).Result;

            Assert.IsFalse(result);
        }
    }

    public class AuthorizationControllerTestFixture
    {
        public AuthorizationControllerTestFixture()
        {
            MediatorMock = new Mock<IMediator>();
            ApprenticeEmailFeatureServiceMock = new Mock<IApprenticeEmailFeatureService>();
            AuthorizationController = new AuthorizationController(MediatorMock.Object, ApprenticeEmailFeatureServiceMock.Object, Mock.Of<ILogger<AuthorizationController>>());
        }

        public Mock<IMediator> MediatorMock { get; }
        public Mock<IApprenticeEmailFeatureService> ApprenticeEmailFeatureServiceMock { get; }
        public AuthorizationController AuthorizationController { get; }

        public AuthorizationControllerTestFixture SetCanAccessCohortToReturnTrue()
        {
            MediatorMock.Setup(x => x.Send(It.IsAny<CanAccessCohortQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            return this;
        }

        public AuthorizationControllerTestFixture SetCanAccessApprenticeshipToReturnTrue()
        {
            MediatorMock.Setup(x => x.Send(It.IsAny<CanAccessApprenticeshipQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            return this;
        }

        public AuthorizationControllerTestFixture SetApprenticeEmailRequiredForProviderToReturnTrue(long providerId)
        {
            ApprenticeEmailFeatureServiceMock.Setup(x => x.ApprenticeEmailIsRequiredFor(providerId)).Returns(true);

            return this;
        }

        public AuthorizationControllerTestFixture SetApprenticeEmailFeatureOnOff(bool onOff)
        {
            ApprenticeEmailFeatureServiceMock.Setup(x => x.IsEnabled).Returns(onOff);

            return this;
        }
    }
}