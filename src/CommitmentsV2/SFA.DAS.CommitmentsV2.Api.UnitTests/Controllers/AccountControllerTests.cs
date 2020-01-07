using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private AccountControllerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AccountControllerTestsFixture();
        }

        [Test]
        public async Task GetAccount_Should_Return_Valid_Result()
        {
            await _fixture.GetAccount();
            _fixture.VerifyResult();
        }

        private class AccountControllerTestsFixture
        {
            private AccountController Controller { get; }
            private Mock<IMediator> Mediator { get; }
            private long AccountId { get; }
            private GetAccountSummaryResponse MediatorResponse { get; }
            private IActionResult Result { get; set; }

            public AccountControllerTestsFixture()
            {
                var autoFixture = new Fixture();

                MediatorResponse = autoFixture.Create<GetAccountSummaryResponse>();
                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetAccountSummaryRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(MediatorResponse);

                Controller = new AccountController(Mediator.Object);
                AccountId = autoFixture.Create<long>();
            }

            public async Task GetAccount()
            {
                Result = await Controller.GetAccount(AccountId);
            }

            public void VerifyResult()
            {
                Assert.AreEqual(typeof(OkObjectResult), Result.GetType());
                var objectResult = (OkObjectResult)Result;
                Assert.AreEqual(200, objectResult.StatusCode);

                Assert.IsInstanceOf<AccountResponse>(objectResult.Value);

                var response = (AccountResponse) objectResult.Value;

                Assert.AreEqual(MediatorResponse.AccountId, response.AccountId);
                Assert.AreEqual(MediatorResponse.HasApprenticeships, response.HasApprenticeships);
                Assert.AreEqual(MediatorResponse.HasCohorts, response.HasCohorts);
            }
        }

    }
}
