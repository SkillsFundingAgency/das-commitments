using System.Linq;
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
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;

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

        [Test]
        public async Task GetApprovedProviders_Should_Return_Valid_Result()
        {
            await _fixture.GetApprovedProviders();
            _fixture.VerifyApprovedProviderResponse();
        }

        private class AccountControllerTestsFixture
        {
            private AccountController Controller { get; }
            private Mock<IMediator> Mediator { get; }
            private long AccountId { get; }
            private GetAccountSummaryResponse MediatorResponse { get; }

            private GetApprovedProvidersQueryResult ApprovedProviderQueryResponse { get; }
            private IActionResult Result { get; set; }

            public AccountControllerTestsFixture()
            {
                var autoFixture = new Fixture();

                MediatorResponse = autoFixture.Create<GetAccountSummaryResponse>();
                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetAccountSummaryRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(MediatorResponse);

                ApprovedProviderQueryResponse = autoFixture.Create<GetApprovedProvidersQueryResult>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprovedProvidersQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ApprovedProviderQueryResponse);

                Controller = new AccountController(Mediator.Object);
                AccountId = autoFixture.Create<long>();
            }

            public async Task GetAccount()
            {
                Result = await Controller.GetAccount(AccountId);
            }

            public async Task GetApprovedProviders()
            {
                Result = await Controller.GetApprovedProviders(AccountId);
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

            public void VerifyApprovedProviderResponse()
            {
                Assert.AreEqual(typeof(OkObjectResult), Result.GetType());
                var objectResult = (OkObjectResult)Result;
                Assert.AreEqual(200, objectResult.StatusCode);

                Assert.IsInstanceOf<GetApprovedProvidersResponse>(objectResult.Value);

                var response = (GetApprovedProvidersResponse)objectResult.Value;

                Assert.AreEqual(3, response.ProviderIds.Length);
                
                foreach (var qr in ApprovedProviderQueryResponse.ProviderIds)
                {
                    Assert.IsTrue(response.ProviderIds.Contains(qr));
                }
            }
        }

    }
}
