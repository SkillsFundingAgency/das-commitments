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
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

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
            private Mock<IModelMapper> ModelMapper { get;  }
            private long AccountId { get; }
            private GetAccountSummaryQueryResult MediatorQueryResult { get; }

            private GetApprovedProvidersQueryResult ApprovedProviderQueryResponse { get; }
            private IActionResult Result { get; set; }

            public AccountControllerTestsFixture()
            {
                var autoFixture = new Fixture();

                MediatorQueryResult = autoFixture.Create<GetAccountSummaryQueryResult>();
                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetAccountSummaryQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(MediatorQueryResult);
                ModelMapper = new Mock<IModelMapper>();

                ApprovedProviderQueryResponse = autoFixture.Create<GetApprovedProvidersQueryResult>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprovedProvidersQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ApprovedProviderQueryResponse);

                Controller = new AccountController(Mediator.Object, ModelMapper.Object);
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

                Assert.AreEqual(MediatorQueryResult.AccountId, response.AccountId);
                Assert.AreEqual(MediatorQueryResult.LevyStatus, response.LevyStatus);
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
