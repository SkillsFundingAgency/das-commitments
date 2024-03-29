﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
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
            Assert.AreEqual(typeof(OkObjectResult), response.GetType());

            var objectResult = (OkObjectResult) response;

            Assert.AreEqual(200, objectResult.StatusCode);
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

            Assert.AreEqual(1, model.AccountId);
            Assert.AreEqual(234, model.MaLegalEntityId);
            Assert.AreEqual("AccountName", model.AccountName);
            Assert.AreEqual("ABC", model.LegalEntityName);
            Assert.AreEqual(ApprenticeshipEmployerType.Levy, model.LevyStatus);
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

            var objectResult = (NotFoundResult) response;

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
            var controller = new AccountLegalEntityController(Logger, Mediator);

            var response = await controller.GetAccountLegalEntity(accountLegalEntityId);

            return response;
        }
    }
}