using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
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
        public async Task GetAccountTransferStatus_Should_Return_Valid_Result()
        {
            await _fixture.GetAccountTransferStatus();
            _fixture.VerifyTransferStatusResponse();
        }

        [Test]
        public async Task GetApprovedProviders_Should_Return_Valid_Result()
        {
            await _fixture.GetApprovedProviders();
            _fixture.VerifyApprovedProviderResponse();
        }

        [Test]
        public async Task GetProviderPaymentPriorities_Should_Return_Valid_Result()
        {
            await _fixture.GetProviderPaymentsPriority();
            _fixture.VerifyGetProviderPaymentsPriorityResponse();
        }

        private class AccountControllerTestsFixture
        {
            private AccountController Controller { get; }
            private Mock<IMediator> Mediator { get; }
            private Mock<IModelMapper> ModelMapper { get;  }
            private long AccountId { get; }
            private GetAccountSummaryQueryResult MediatorQueryResult { get; }

            private GetAccountTransferStatusQueryResult AccountTransferStatusQueryResult { get; }

            private GetApprovedProvidersQueryResult ApprovedProviderQueryResult { get; }

            private GetProviderPaymentsPriorityQueryResult ProviderPaymentsPriorityQueryResult { get; }
            private GetProviderPaymentsPriorityResponse GetProviderPaymentsPriorityResponse { get; }

            private IActionResult Result { get; set; }

            public AccountControllerTestsFixture()
            {
                var autoFixture = new Fixture();

                MediatorQueryResult = autoFixture.Create<GetAccountSummaryQueryResult>();
                Mediator = new Mock<IMediator>();
                Mediator.Setup(x => x.Send(It.IsAny<GetAccountSummaryQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(MediatorQueryResult);

                AccountTransferStatusQueryResult = autoFixture.Create<GetAccountTransferStatusQueryResult>();
                Mediator.Setup(x => x.Send(It.IsAny<GetAccountTransferStatusQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(AccountTransferStatusQueryResult);

                ApprovedProviderQueryResult = autoFixture.Create<GetApprovedProvidersQueryResult>();
                Mediator.Setup(x => x.Send(It.IsAny<GetApprovedProvidersQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ApprovedProviderQueryResult);

                ModelMapper = new Mock<IModelMapper>();
                
                ProviderPaymentsPriorityQueryResult = new GetProviderPaymentsPriorityQueryResult
                {
                    PriorityItems = new List<GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem>
                    {
                        new GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem { PriorityOrder = 1, ProviderId = 123, ProviderName = "Test1" },
                        new GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem { PriorityOrder = 2, ProviderId = 456, ProviderName = "Test2" },
                        new GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem { PriorityOrder = 3, ProviderId = 789, ProviderName = "Test3" }
                    }
                };

                GetProviderPaymentsPriorityResponse = new GetProviderPaymentsPriorityResponse
                {
                    ProviderPaymentPriorities = TestHelpers
                    .CloneHelper
                    .Clone<List<GetProviderPaymentsPriorityQueryResult.ProviderPaymentsPriorityItem>, List<GetProviderPaymentsPriorityResponse.ProviderPaymentPriorityItem>>
                    (
                        ProviderPaymentsPriorityQueryResult.PriorityItems.ToList()
                    )
                };

                ModelMapper.Setup(x => x.Map<GetProviderPaymentsPriorityResponse>(ProviderPaymentsPriorityQueryResult)).ReturnsAsync(GetProviderPaymentsPriorityResponse);

                Mediator.Setup(x => x.Send(It.IsAny<GetProviderPaymentsPriorityQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ProviderPaymentsPriorityQueryResult);

                Controller = new AccountController(Mediator.Object, ModelMapper.Object);
                AccountId = autoFixture.Create<long>();
            }

            public async Task GetAccount()
            {
                Result = await Controller.GetAccount(AccountId);
            }

            public async Task GetAccountTransferStatus()
            {
                Result = await Controller.GetAccountTransferStatus(AccountId);
            }

            public async Task GetApprovedProviders()
            {
                Result = await Controller.GetApprovedProviders(AccountId);
            }

            public async Task GetProviderPaymentsPriority()
            {
                Result = await Controller.GetProviderPaymentsPriority(AccountId);
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

            public void VerifyTransferStatusResponse()
            {
                Assert.AreEqual(typeof(OkObjectResult), Result.GetType());
                var objectResult = (OkObjectResult)Result;
                Assert.AreEqual(200, objectResult.StatusCode);

                var response = (AccountTransferStatusResponse)objectResult.Value;

                Assert.AreEqual(AccountTransferStatusQueryResult.IsTransferSender, response.IsTransferSender);
                Assert.AreEqual(AccountTransferStatusQueryResult.IsTransferReceiver, response.IsTransferReceiver);
            }

            public void VerifyApprovedProviderResponse()
            {
                Assert.AreEqual(typeof(OkObjectResult), Result.GetType());
                var objectResult = (OkObjectResult)Result;
                Assert.AreEqual(200, objectResult.StatusCode);

                Assert.IsInstanceOf<GetApprovedProvidersResponse>(objectResult.Value);

                var response = (GetApprovedProvidersResponse)objectResult.Value;

                Assert.AreEqual(3, response.ProviderIds.Length);
                
                foreach (var qr in ApprovedProviderQueryResult.ProviderIds)
                {
                    Assert.IsTrue(response.ProviderIds.Contains(qr));
                }
            }

            public void VerifyGetProviderPaymentsPriorityResponse()
            {
                Assert.AreEqual(typeof(OkObjectResult), Result.GetType());
                var objectResult = (OkObjectResult)Result;
                Assert.AreEqual(200, objectResult.StatusCode);

                Assert.IsInstanceOf<GetProviderPaymentsPriorityResponse>(objectResult.Value);

                var response = (GetProviderPaymentsPriorityResponse)objectResult.Value;

                Assert.AreEqual(3, response.ProviderPaymentPriorities.Count);

                foreach (var qr in ProviderPaymentsPriorityQueryResult.PriorityItems)
                {
                    Assert.IsTrue(response.ProviderPaymentPriorities.All(p => ProviderPaymentsPriorityQueryResult.PriorityItems
                        .Any(a => a.PriorityOrder == p.PriorityOrder && a.ProviderId == p.ProviderId && a.ProviderName == p.ProviderName)));
                }
            }
        }

    }
}
