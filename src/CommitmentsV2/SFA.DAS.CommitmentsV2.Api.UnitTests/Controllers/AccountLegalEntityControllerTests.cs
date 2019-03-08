using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Queries.GetAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.UnitTests.Queries.GetEmployer
{
    [TestFixture]
    public class AccountLegalEntityControllerTests
    {
        [Test]
        public async Task GetAccountLegalEntity_WithValidModelAndExistingId_ShouldReturnOkayAndContent()
        {
            const long accountLegalEntityId = 456;

            // arrange
            var fixtures = new AccountLegalEntityControllerTestFixtures()
                .SetQueryResponse(accountLegalEntityId, new GetAccountLegalEntityResponse { AccountName = "AccountName", LegalEntityName = "" });

            // act
            var response = await fixtures.CallControllerMethod(accountLegalEntityId);

            // Assert
            Assert.AreEqual(typeof(OkObjectResult), response.GetType());

            var objectResult = response as OkObjectResult;

            Assert.AreEqual(200, objectResult.StatusCode);
        }

        [Test]
        public async Task GetAccountLegalEntity_WithValidModelButInvalidId_ShouldReturnNotFound()
        {
            const long accountLegalEntityId = 456;

            // arrange
            var fixtures = new AccountLegalEntityControllerTestFixtures()
                .SetQueryResponse(accountLegalEntityId, null);

            // act
            var response = await fixtures.CallControllerMethod(accountLegalEntityId);

            // Assert
            Assert.AreEqual(typeof(NotFoundResult), response.GetType());

            var objectResult = response as NotFoundResult;

            Assert.AreEqual(404, objectResult.StatusCode);
        }
    }


    public class AccountLegalEntityControllerTestFixtures
    {
        public AccountLegalEntityControllerTestFixtures()
        {
            MediatorMock = new Mock<IMediator>();
            LoggerMock = new Mock<ILogger<AccountLegalEntityController>>();
        }

        public Mock<IMediator> MediatorMock { get; set; }
        public IMediator Mediator => MediatorMock.Object;

        public Mock<ILogger<AccountLegalEntityController>> LoggerMock { get; set; }
        public ILogger<AccountLegalEntityController> Logger => LoggerMock.Object;

        public AccountLegalEntityControllerTestFixtures SetQueryResponse(long forAccountLegalEntityId, GetAccountLegalEntityResponse sendResponse)
        {
            MediatorMock
                .Setup(m => m.Send(
                                    It.Is<GetAccountLegalEntityRequest>(request => request.AccountLegalEntityId == forAccountLegalEntityId),
                                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(sendResponse));

            return this;
        }

        public async Task<IActionResult> CallControllerMethod(long accountLegalEntityId)
        {
            var controller = new AccountLegalEntityController(Logger, Mediator);

            var response = await controller.GetAccountLegalEntity(accountLegalEntityId);

            return response;
        }
    }
}
