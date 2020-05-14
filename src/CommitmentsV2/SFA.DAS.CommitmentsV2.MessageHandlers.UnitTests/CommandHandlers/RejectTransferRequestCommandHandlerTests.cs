using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class RejectTransferRequestCommandHandlerTests
    {
        private RejectTransferRequestCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new RejectTransferRequestCommandHandlerTestsFixture();
        }

        [Test]
        public void WhenHandlingRejectTransferRequestCommand_AndDbDoesNotHaveRecord_ThenExceptionThrownAndLogged()
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.Act());
            _fixture.VerifyHasError();
        }

        [Test]
        public async Task WhenHandlingRejectTransferRequestCommand_AndDbHasRecord_ThenNoExceptionThrown()
        {
            _fixture.WithTransferRequestAndCohortInDatabase();

            await _fixture.Act();
        }

        [TestCase(TransferApprovalStatus.Pending, false)]
        [TestCase(TransferApprovalStatus.Approved, true)]
        public async Task WhenCallingRejectOnTransferRequest_ThenTransferApprovalStatusHandledCorrectly(TransferApprovalStatus status, bool shouldThrowException)
        {
            _fixture
                .WithTransferRequestAndCohortInDatabase()
                .WithTransferApprovalStatus(status);

            if (shouldThrowException)
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await _fixture.Act());
            }
            else
            {
                await _fixture.Act();
            }
        }

        [Test]
        public async Task WhenCallingRejectOnTransferRequestAndAlreadyBeenRejected_ThenShouldSwallowMessageAndLogWarning()
        {
            _fixture
                .WithTransferRequestAndCohortInDatabase()
                .WithTransferApprovalStatus(TransferApprovalStatus.Rejected);

            await _fixture.Act();
            _fixture.VerifyHasWarning();
        }

        [Test]
        public async Task WhenCallingRejectOnTransferRequest_ThenEntityIsTracked()
        {
            _fixture
                .WithTransferRequestAndCohortInDatabase()
                .WithTransferApprovalStatus(TransferApprovalStatus.Pending);

            await _fixture.Act();

            _fixture.VerifyEntityIsBeingTracked();
        }

        [Test]
        public async Task WhenCallingRejectOnTransferRequest_ThenTransferRequestApprovalStatusUpdatedInDatabase()
        {
            _fixture
                .WithTransferRequestAndCohortInDatabase()
                .WithTransferApprovalStatus(TransferApprovalStatus.Pending);

            await _fixture.Act();

            _fixture.VerifyTransferApprovalStatusUpdatedToRejected();
        }

        [Test]
        public async Task WhenCallingRejectOnTransferRequest_ThenPublishesTransferRequestRejectedEvent()
        {
            _fixture
                .WithTransferRequestAndCohortInDatabase()
                .WithTransferApprovalStatus(TransferApprovalStatus.Pending);

            await _fixture.Act();

            _fixture.VerifyTransferRequestRejectedEventIsPublished();
        }
    }

    public class RejectTransferRequestCommandHandlerTestsFixture
    {
        private long _transferRequestId;
        private UserInfo _userInfo;
        private RejectTransferRequestCommand _command;
        private TransferRequest _transferRequest;
        private Cohort _cohort;
        private ProviderCommitmentsDbContext _db;
        private FakeLogger<RejectTransferRequestCommandHandler> _logger;
        private Mock<IMessageHandlerContext> _mockMessageHandlerContext;
        private RejectTransferRequestCommandHandler _sut;
        private UnitOfWorkContext _unitOfWorkContext;
        private DateTime _rejectedOnDate;

        public RejectTransferRequestCommandHandlerTestsFixture()
        {
            _transferRequestId = 235;
            _userInfo = new UserInfo
            {
                UserDisplayName = "TestName",
                UserEmail = "TestEmail@Test.com",
                UserId = "23432"
            };
            _transferRequest = new TransferRequest { Id = _transferRequestId };
            _cohort = new Cohort
            {
                TransferRequests = { _transferRequest },
                EmployerAccountId = 8787,
                AccountLegalEntityId = 234,
                //AccountLegalEntityPublicHashedId = "sfs1",
                ProviderId = 234234,
                TransferSenderId = 8999

            };
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            _logger = new FakeLogger<RejectTransferRequestCommandHandler>();
            _mockMessageHandlerContext = new Mock<IMessageHandlerContext>();
            _rejectedOnDate = new DateTime(2020, 01, 30, 14, 22, 00);
            _command = new RejectTransferRequestCommand(_transferRequestId, _rejectedOnDate, _userInfo);
            _unitOfWorkContext = new UnitOfWorkContext();
            _sut = new RejectTransferRequestCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), _logger);
        }

        public async Task Act() => await _sut.Handle(_command, _mockMessageHandlerContext.Object);

        public RejectTransferRequestCommandHandlerTestsFixture WithTransferRequestAndCohortInDatabase()
        {
            _db.Cohorts.Add(_cohort);
            _db.SaveChanges();
            return this;
        }

        public RejectTransferRequestCommandHandlerTestsFixture WithTransferApprovalStatus(TransferApprovalStatus status)
        {
            _transferRequest.Status = status;
            return this;
        }

        public void VerifyTransferApprovalStatusUpdatedToRejected()
        {
            var transferRequest = _db.TransferRequests
                .SingleOrDefault(x => x.Id == _transferRequestId);

            if (transferRequest == null) Assert.Fail("TransferRequest not in database.");
            Assert.AreEqual(TransferApprovalStatus.Rejected, transferRequest.Status);
        }

        public void VerifyTransferRequestRejectedEventIsPublished()
        {
            var list = _unitOfWorkContext.GetEvents()
                .OfType<TransferRequestRejectedEvent>()
                .ToList();

            var rejectedEvent = list
                .SingleOrDefault(x => x.TransferRequestId == _transferRequestId);

            Assert.NotNull(rejectedEvent);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(_cohort.Id, rejectedEvent.CohortId);
            Assert.AreEqual(_rejectedOnDate, rejectedEvent.RejectedOn);
            Assert.AreSame(_userInfo, rejectedEvent.UserInfo);
        }

        public void VerifyEntityIsBeingTracked()
        {
            var list = _unitOfWorkContext.GetEvents()
                .OfType<EntityStateChangedEvent>()
                .ToList();

            var rejectedEvent = list[0];

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual(UserAction.RejectTransferRequest, rejectedEvent.StateChangeType);
            Assert.AreEqual(_transferRequestId, rejectedEvent.EntityId);
            Assert.AreEqual(_userInfo.UserId, rejectedEvent.UpdatingUserId);
            Assert.AreEqual(_userInfo.UserDisplayName, rejectedEvent.UpdatingUserName);
            Assert.AreEqual(Party.TransferSender, rejectedEvent.UpdatingParty);
        }

        public void VerifyHasError()
        {
            Assert.IsTrue(_logger.HasErrors);
        }

        public void VerifyHasWarning()
        {
            Assert.IsTrue(_logger.HasWarnings);
        }
    }
}