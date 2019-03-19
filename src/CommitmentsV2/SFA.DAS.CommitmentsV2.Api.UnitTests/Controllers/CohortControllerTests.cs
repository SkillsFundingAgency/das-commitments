using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class CohortControllerTests
    {
        [Test]
        public Task CreateController_ValidRequest_ShouldReturnAnOkResult()
        {
            return new CohortControllerTestFixtures()
                        .AssertOkayResponse();
        }

        [Test]
        public Task CreateController_ValidRequest_ShouldReturnExpectedExpectedResponseObject()
        {
            return new CohortControllerTestFixtures()
                        .AssertExpectedResponseType();
        }

        [Test]
        public Task CreateController_ValidRequest_ShouldReturnExpectedCohortId()
        {
            return new CohortControllerTestFixtures()
                .AssertResponseIsValid(response => 
                    Assert.AreEqual(CohortControllerTestFixtures.CohortId, response.Id));
        }

        [Test]
        public Task CreateController_ValidRequest_ShouldReturnExpectedReference()
        {
            return new CohortControllerTestFixtures()
                .AssertResponseIsValid(response =>
                    Assert.AreEqual(CohortControllerTestFixtures.Reference, response.Reference));
        }

        [Test]
        public Task CreateController_ValidRequest_ShouldReturnExpectedApprenticeshipId()
        {
            return new CohortControllerTestFixtures()
                .AssertResponseIsValid(response => 
                    Assert.AreEqual(CohortControllerTestFixtures.DraftApprenticeshipId, response.DraftApprenticeshipId));
        }
    }

    public class CohortControllerTestFixtures
    {
        public const long CohortId = 123;
        public const string Reference = "ABC123";
        public const long DraftApprenticeshipId = 456;

        public CohortControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            MapperMock = new Mock<IMapper<CreateCohortRequest, AddCohortCommand>>();
        }

        private Mock<IMediator> MediatorMock { get; }
        private IMediator Mediator => MediatorMock.Object;

        private Mock<IMapper<CreateCohortRequest, AddCohortCommand>> MapperMock { get; }
        private IMapper<CreateCohortRequest, AddCohortCommand> Mapper => MapperMock.Object;

        public CohortController CreateController()
        {
            return new CohortController(Mediator, Mapper);
        }

        public CohortControllerTestFixtures WithCommandResponse(long id, string reference, long draftApprenticeshipId)
        {
            MediatorMock
                .Setup(m => m.Send(It.IsAny<AddCohortCommand>(), CancellationToken.None))
                .ReturnsAsync(new AddCohortResponse
                {
                    DraftApprenticeshipId = draftApprenticeshipId,
                    Id = id,
                    Reference = reference
                });

            return this;
        }

        public async Task<OkObjectResult> AssertOkayResponse()
        {
            var fixtures = new CohortControllerTestFixtures()
                .WithCommandResponse(CohortId, Reference, DraftApprenticeshipId);

            //Arrange
            var controller = fixtures.CreateController();

            //Act
            var response = await controller.CreateCohort(new CreateCohortRequest());

            //Assert
            Assert.IsTrue(response is OkObjectResult);

            return (OkObjectResult)response;
        }

        public async Task<AddCohortResponse> AssertExpectedResponseType()
        {
            var okResponse = await AssertOkayResponse();

            //Assert
            Assert.IsTrue(okResponse.Value is AddCohortResponse);

            return (AddCohortResponse)okResponse.Value;
        }

        public async Task AssertResponseIsValid(Action<AddCohortResponse> assert)
        {
            var responseBody = await AssertExpectedResponseType();

            //Assert
            assert(responseBody);
        }
    }
}