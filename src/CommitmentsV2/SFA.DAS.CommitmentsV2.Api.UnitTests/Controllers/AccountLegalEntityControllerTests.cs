using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
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
                .SetQueryResponse(accountLegalEntityId, new GetAccountLegalEntityQueryResult());

            // act
            var response = await fixtures.CallControllerMethod(accountLegalEntityId);

            // Assert
            Assert.That(response.GetType(), Is.EqualTo(typeof(OkObjectResult)));

            var objectResult = (OkObjectResult) response;

            Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetAccountLegalEntity_WithValidModelAndExistingId_ShouldResultMappedCorrectly()
        {
            const long accountLegalEntityId = 456;

            // arrange
            var fixtures = new AccountLegalEntityControllerTestFixtures()
                .SetQueryResponse(accountLegalEntityId, new GetAccountLegalEntityQueryResult { AccountId = 1, MaLegalEntityId = 234, AccountName = "AccountName", LegalEntityName = "ABC", LevyStatus = ApprenticeshipEmployerType.Levy });

            // act
            var response = await fixtures.CallControllerMethod(accountLegalEntityId);

            // Assert
            var model = response
                .VerifyReturnsModel()
                .WithModel<AccountLegalEntityResponse>();

            Assert.Multiple(() =>
            {
                Assert.That(model.AccountId, Is.EqualTo(1));
                Assert.That(model.MaLegalEntityId, Is.EqualTo(234));
                Assert.That(model.AccountName, Is.EqualTo("AccountName"));
                Assert.That(model.LegalEntityName, Is.EqualTo("ABC"));
                Assert.That(model.LevyStatus, Is.EqualTo(ApprenticeshipEmployerType.Levy));
            });
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
            Assert.That(response.GetType(), Is.EqualTo(typeof(NotFoundResult)));

            var objectResult = (NotFoundResult) response;

            Assert.That(objectResult.StatusCode, Is.EqualTo(404));
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

        public AccountLegalEntityControllerTestFixtures SetQueryResponse(long forAccountLegalEntityId, GetAccountLegalEntityQueryResult sendQueryResult)
        {
            MediatorMock
                .Setup(m => m.Send(
                                    It.Is<GetAccountLegalEntityQuery>(request => request.AccountLegalEntityId == forAccountLegalEntityId),
                                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(sendQueryResult));

            return this;
        }

        public async Task<IActionResult> CallControllerMethod(long accountLegalEntityId)
        {
            var controller = new AccountLegalEntityController(Mediator);

            var response = await controller.GetAccountLegalEntity(accountLegalEntityId);

            return response;
        }
    }
}