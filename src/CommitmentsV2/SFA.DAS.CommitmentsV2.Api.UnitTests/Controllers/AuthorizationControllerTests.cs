using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using AutoFixture;
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

        [TestCase(PartyType.Provider, "123", 1, null, 123L)]
        [TestCase(PartyType.Employer, "123", 1, 123L, null)]
        [TestCase(PartyType.Provider, "NotANumber", 2, null, null)]
        [TestCase(PartyType.Employer, "NotANumber", 2, null, null)]
        public async Task AuthorizationController_AccessCohortRequest_ShouldCallCommandWithCorrectQueryValues(
            PartyType partyType, string partyId, long cohortId, long? expectedAccountId, long? expectedProviderId)
        {
            await _fixture.AuthorizationController.CanAccessCohort(partyType, partyId, cohortId);

            _fixture.MediatorMock.Verify(x => x.Send(It.Is<CanAccessCohortQuery>(p => p.CohortId == cohortId &&
                                                                                      p.PartyType == partyType &&
                                                                                      p.AccountId ==
                                                                                      expectedAccountId &&
                                                                                      p.ProviderId ==
                                                                                      expectedProviderId),
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