using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Types;

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

        [TestCase(PartyType.Provider, 124, 1)]
        [TestCase(PartyType.Employer, 123, 2)]
        public async Task AuthorizationController_AccessCohortRequest_ShouldCallCommandWithCorrectQueryValues(
            PartyType partyType, long partyId, long cohortId)
        {
            var request = new CohortAccessRequest {CohortId = cohortId, PartyType = partyType, PartyId = partyId};

            await _fixture.AuthorizationController.CanAccessCohort(request);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<CanAccessCohortQuery>(p => p.CohortId == cohortId &&
                                                                                      p.PartyType == partyType &&
                                                                                      p.PartyId == partyId),
                CancellationToken.None), Times.Once);
        }
    }

    public class AuthorizationControllerTestFixture
    {
        public AuthorizationControllerTestFixture()
        {
            var fixture = new Fixture();
            MediatorMock = new Mock<IMediator>();

            AuthorizationController = new AuthorizationController(MediatorMock.Object);
        }

        public Mock<IMediator> MediatorMock { get; }

        public AuthorizationController AuthorizationController { get; }
    }
}