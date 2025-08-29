using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

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
    public async Task GetAccountStatus_Should_Return_Valid_Result()
    {
        await _fixture.GetAccountStatus();
        _fixture.VerifyAccountStatusRepsonse();
    }

    [Test]
    public async Task GetAccount_Should_Return_Valid_Result()
    {
        await _fixture.GetAccount();
        _fixture.VerifyAccountRepsonse();
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
        private Mock<IModelMapper> ModelMapper { get; }
        private long AccountId { get; }

        private GetAccountStatusQueryResult AccountStatusQueryResult { get; }
        private GetAccountSummaryQueryResult AccountSummaryQueryResult { get; }

        private GetAccountTransferStatusQueryResult AccountTransferStatusQueryResult { get; }

        private GetApprovedProvidersQueryResult ApprovedProviderQueryResult { get; }

        private GetProviderPaymentsPriorityQueryResult ProviderPaymentsPriorityQueryResult { get; }
        private GetProviderPaymentsPriorityResponse GetProviderPaymentsPriorityResponse { get; }

        private IActionResult Result { get; set; }

        public AccountControllerTestsFixture()
        {
            var autoFixture = new Fixture();

            AccountStatusQueryResult = autoFixture.Create<GetAccountStatusQueryResult>();
            Mediator = new Mock<IMediator>();
            Mediator.Setup(x => x.Send(It.IsAny<GetAccountStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccountStatusQueryResult);

            AccountSummaryQueryResult = autoFixture.Create<GetAccountSummaryQueryResult>();
            Mediator.Setup(x => x.Send(It.IsAny<GetAccountSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccountSummaryQueryResult);

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
                    new() { PriorityOrder = 1, ProviderId = 123, ProviderName = "Test1" },
                    new() { PriorityOrder = 2, ProviderId = 456, ProviderName = "Test2" },
                    new() { PriorityOrder = 3, ProviderId = 789, ProviderName = "Test3" }
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

        public async Task GetAccountStatus()
        {
            Result = await Controller.GetAccountStatus(AccountId, 0, 0, 0);
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

        public void VerifyAccountRepsonse()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));

                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));
                Assert.That(objectResult.Value, Is.InstanceOf<AccountResponse>());

                var response = (AccountResponse)objectResult.Value;
                Assert.That(response.AccountId, Is.EqualTo(AccountSummaryQueryResult.AccountId));
                Assert.That(response.LevyStatus, Is.EqualTo(AccountSummaryQueryResult.LevyStatus));
            });
        }

        public void VerifyAccountStatusRepsonse()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));

                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));
                Assert.That(objectResult.Value, Is.InstanceOf<AccountStatusResponse>());

                var response = (AccountStatusResponse)objectResult.Value;
                Assert.That(response.Completed, Is.EqualTo(AccountStatusQueryResult.Completed));
                Assert.That(response.NewStart, Is.EqualTo(AccountStatusQueryResult.NewStart));
                Assert.That(response.Active, Is.EqualTo(AccountStatusQueryResult.Active));
            });
        }

        public void VerifyTransferStatusResponse()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));
                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));

                var response = (AccountTransferStatusResponse)objectResult.Value;

                Assert.That(response.IsTransferSender, Is.EqualTo(AccountTransferStatusQueryResult.IsTransferSender));
                Assert.That(response.IsTransferReceiver, Is.EqualTo(AccountTransferStatusQueryResult.IsTransferReceiver));
            });
        }

        public void VerifyApprovedProviderResponse()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));
                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));

                Assert.That(objectResult.Value, Is.InstanceOf<GetApprovedProvidersResponse>());

                var response = (GetApprovedProvidersResponse)objectResult.Value;

                Assert.That(response.ProviderIds, Has.Length.EqualTo(3));

                foreach (var qr in ApprovedProviderQueryResult.ProviderIds)
                {
                    Assert.That(response.ProviderIds, Does.Contain(qr));
                }
            });
        }

        public void VerifyGetProviderPaymentsPriorityResponse()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));
                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));

                Assert.That(objectResult.Value, Is.InstanceOf<GetProviderPaymentsPriorityResponse>());

                var response = (GetProviderPaymentsPriorityResponse)objectResult.Value;

                Assert.That(response.ProviderPaymentPriorities, Has.Count.EqualTo(3));

                foreach (var qr in ProviderPaymentsPriorityQueryResult.PriorityItems)
                {
                    Assert.That(response.ProviderPaymentPriorities.All(p => ProviderPaymentsPriorityQueryResult.PriorityItems
                        .Any(a => a.PriorityOrder == p.PriorityOrder && a.ProviderId == p.ProviderId && a.ProviderName == p.ProviderName)));
                }
            });
        }
    }
}