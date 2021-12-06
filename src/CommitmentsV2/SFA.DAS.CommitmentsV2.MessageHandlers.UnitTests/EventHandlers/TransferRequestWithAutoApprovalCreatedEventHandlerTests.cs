using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    [Parallelizable]
    public class TransferRequestWithAutoApprovalCreatedEventHandlerTests
    {
        private TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture();
        }

        [TestCase(1, 100, TransferApprovalStatus.Approved)]
        [TestCase(100, 100, TransferApprovalStatus.Approved)]
        [TestCase(101, 100, TransferApprovalStatus.Rejected)]
        public async Task Handle_WhenHandlingEvent_TransferRequestIsApprovedIfPledgeApplicationHasSufficientFunds(int requestAmount, int fundsRemaining, TransferApprovalStatus expectedStatus)
        {
            _fixture
                .WithTransferRequest(requestAmount)
                .WithPledgeApplication(fundsRemaining);

            await _fixture.Handle();

            _fixture.VerifyTransferRequestStatus(expectedStatus);
        }

        public class TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture
        {
            public Fixture Fixture { get; private set; }
            private TransferRequestWithAutoApprovalCreatedEventHandler _handler;
            public TransferRequestWithAutoApprovalCreatedEvent _event;
            public TransferRequest TransferRequest { get; private set; }
            public PledgeApplication PledgeApplication { get; private set; }
            public int PledgeApplicationId { get; private set; }
            public Mock<IApiClient> LevyTransferMatchingApiClient { get; private set; }
            public ProviderCommitmentsDbContext Db { get; set; }
            public UnitOfWorkContext UnitOfWorkContext { get; set; }

            public TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture()
            {
                Fixture = new Fixture();
                UnitOfWorkContext = new UnitOfWorkContext();
                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

                PledgeApplicationId = Fixture.Create<int>();
                _event = new TransferRequestWithAutoApprovalCreatedEvent(1, PledgeApplicationId, DateTime.UtcNow);
                PledgeApplication = new PledgeApplication();
                LevyTransferMatchingApiClient = new Mock<IApiClient>();
                LevyTransferMatchingApiClient.Setup(x => x.Get<PledgeApplication>(It.IsAny<GetPledgeApplicationRequest>())).ReturnsAsync(PledgeApplication);

                _handler = new TransferRequestWithAutoApprovalCreatedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                    Mock.Of<ILogger<TransferRequestWithAutoApprovalCreatedEventHandler>>(),
                    LevyTransferMatchingApiClient.Object);
            }

            public TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture WithTransferRequest(int requestAmount)
            {
                TransferRequest = new TransferRequest("", 1, requestAmount, true)
                {
                    Id = _event.TransferRequestId,
                    Cohort = new Cohort { PledgeApplicationId = PledgeApplicationId }
                };
                Db.TransferRequests.Add(TransferRequest);
                Db.SaveChanges();
                return this;
            }

            public TransferRequestWithAutoApprovalCreatedEventHandlerTestsFixture WithPledgeApplication(int fundsRemaining)
            {
                PledgeApplication.AmountRemaining = fundsRemaining;
                PledgeApplication.TotalAmount = fundsRemaining;
                PledgeApplication.AmountUsed = 0;
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, Mock.Of<IMessageHandlerContext>());
            }

            public void VerifyTransferRequestStatus(TransferApprovalStatus expectedStatus)
            {
                Assert.AreEqual(expectedStatus, TransferRequest.Status);
            }
        }
    }
}
