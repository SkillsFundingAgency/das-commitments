using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CohortControllerTests
    {
        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnAnOkResult()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(response is OkObjectResult);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();

            //Assert
            Assert.IsTrue(((OkObjectResult)response).Value is CreateCohortResponse);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedCohortId()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.CohortId, addCohortResponse.CohortId);
        }

        [Test]
        public async Task CreateCohort_ValidRequest_ShouldReturnExpectedReference()
        {
            //Arrange
            var fixture = new CohortControllerTestFixtures().WithAddCohortCommandResponse();

            //Act
            var response = await fixture.CreateCohort();
            var addCohortResponse = ((OkObjectResult)response).Value as CreateCohortResponse;

            //Assert
            Assert.AreEqual(CohortControllerTestFixtures.CohortReference, addCohortResponse.CohortReference);
        }


        [Test]
        public Task GetCohort_ValidRequest_ShouldReturnCohortId()
        {
            const long cohortId = 1234;

            return new CohortControllerTestFixtures()
                .AssertGetCohortResponse(
                    cohortId,
                    new GetCohortSummaryResponse {CohortId = cohortId},
                    response => Assert.AreEqual(cohortId, response.CohortId));
        }

        [Test]
        public Task GetCohort_ValidRequest_ShouldReturnLegalEntityName()
        {
            const long cohortId = 1234;
            const string name = "ACME Fireworks";

            return new CohortControllerTestFixtures()
                .AssertGetCohortResponse(
                    cohortId,
                    new GetCohortSummaryResponse { LegalEntityName = name},
                    response => Assert.AreEqual(name, response.LegalEntityName));
        }

        [Test]
        public Task GetCohort_ValidRequest_ShouldReturnProviderName()
        {
            const long cohortId = 1234;
            const string name = "ACME Training";

            return new CohortControllerTestFixtures()
                .AssertGetCohortResponse(
                    cohortId,
                    new GetCohortSummaryResponse { ProviderName = name },
                    response => Assert.AreEqual(name, response.ProviderName));
        }

        [TestCase(true)]
        [TestCase(false)]
        public Task GetCohort_ValidRequest_ShouldReturnIsTransferFunded(bool expectedIsTransferFunded)
        {
            const long cohortId = 1234;

            return new CohortControllerTestFixtures()
                .AssertGetCohortResponse(
                    cohortId,
                    new GetCohortSummaryResponse { IsFundedByTransfer = expectedIsTransferFunded},
                    response => Assert.AreEqual(expectedIsTransferFunded, response.IsFundedByTransfer));
        }
    }

    public class CohortControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string CohortReference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public CohortControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            CreateCohortRequestToAddCohortCommandMapperMock = new Mock<IMapper<CreateCohortRequest, AddCohortCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;
        private Mock<IMapper<CreateCohortRequest, AddCohortCommand>> CreateCohortRequestToAddCohortCommandMapperMock { get; }
        private IMapper<CreateCohortRequest, AddCohortCommand> CreateCohortRequestToAddCohortCommandMapper => CreateCohortRequestToAddCohortCommandMapperMock.Object;

        public CohortController CreateController()
        {
            return new CohortController(Mediator, CreateCohortRequestToAddCohortCommandMapper);
        }

        public CohortControllerTestFixtures WithAddCohortCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddCohortCommand>(), CancellationToken.None))
                .ReturnsAsync(new AddCohortResponse
                {
                    Id = id,
                    Reference = reference
                });

            return this;
        }

        public CohortControllerTestFixtures WithAddCohortCommandResponse()
        {
            return WithAddCohortCommandResponse(CohortId, CohortReference, DraftApprenticeshipId);
        }

        public Task<IActionResult> CreateCohort()
        {
            var controller = CreateController();

            return controller.CreateCohort(new CreateCohortRequest());
        }

        public async Task AssertGetCohortResponse(long cohortId, GetCohortSummaryResponse queryResponse, Action<GetCohortResponse> checkHttpResponse)
        {
            var controller = CreateController();

            MediatorMock
                .Setup(m => m.Send(It.Is<GetCohortSummaryRequest>(r => r.CohortId == cohortId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => queryResponse);
                    
            var http = await controller.GetCohort(cohortId);
            var getCohortResponse = ((OkObjectResult)http).Value as GetCohortResponse;

            checkHttpResponse(getCohortResponse);
        }
    }
}