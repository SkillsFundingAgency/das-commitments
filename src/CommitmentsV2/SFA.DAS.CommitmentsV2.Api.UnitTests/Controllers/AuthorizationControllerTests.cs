﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
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

            var retVal = await _fixture.SetFixtureToReturnTrue().AuthorizationController.CanAccessCohort(request);

            Assert.IsInstanceOf<OkObjectResult>(retVal);
            Assert.IsTrue((bool)((OkObjectResult)retVal).Value);

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

        public AuthorizationControllerTestFixture SetFixtureToReturnTrue()
        {
            MediatorMock.Setup(x => x.Send(It.IsAny<CanAccessCohortQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            return this;
        }
    }
}