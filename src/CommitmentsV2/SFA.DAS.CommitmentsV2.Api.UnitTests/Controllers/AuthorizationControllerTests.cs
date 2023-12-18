using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional;
using SFA.DAS.CommitmentsV2.Services;

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

            Assert.That(retVal, Is.InstanceOf<OkObjectResult>());
            Assert.That((bool)((OkObjectResult)retVal).Value);
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

            Assert.That(retVal, Is.InstanceOf<OkObjectResult>());
            Assert.That((bool)((OkObjectResult)retVal).Value);
        }

        [Test]
        public async Task AuthorizationController_ApprenticeEmailRequired_ShouldSendCorrectQuery()
        {
            var providerId = 123456;

            await _fixture.AuthorizationController.OptionalEmail(0, providerId);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<GetEmailOptionalQuery>(q =>
                q.EmployerId == 0 && q.ProviderId == providerId), CancellationToken.None), Times.Once);
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
        public async Task AuthorizationController_email_optional_test_handler_positive(long employerId, long providerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = new long[] { 123, 456, 789 }, EmailOptionalProviders = new long[] { 321, 654, 987 } };
            var service = new EmailOptionalService(config);
            var sut = new GetEmailOptionalQueryHandler(service);
            var query = new GetEmailOptionalQuery(employerId, providerId);

            var result = await sut.Handle(query, new CancellationToken());

            Assert.That(result);
        }

        [TestCase(78901, 10)]
        [TestCase(0, 456)]
        [TestCase(987, 0)]
        public async Task AuthorizationController_email_optional_test_handler_negative(long employerId, long providerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = new long[] { 123, 456, 789 }, EmailOptionalProviders = new long[] { 321, 654, 987 } };
            var service = new EmailOptionalService(config);
            var sut = new GetEmailOptionalQueryHandler(service);
            var query = new GetEmailOptionalQuery(employerId, providerId);

            var result = await sut.Handle(query, new CancellationToken());

            Assert.IsFalse(result);
        }
    }

    public class AuthorizationControllerTestFixture
    {
        public AuthorizationControllerTestFixture()
        {
            MediatorMock = new Mock<IMediator>();            
            AuthorizationController = new AuthorizationController(MediatorMock.Object, Mock.Of<ILogger<AuthorizationController>>());
        }

        public Mock<IMediator> MediatorMock { get; }        
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
    }
}