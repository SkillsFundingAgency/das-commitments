﻿using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TransferRequestDomainServiceTests
    {
        private TransferRequestDomainServiceTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new TransferRequestDomainServiceTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture.TearDown();
            _fixture = null;
        }

        [Test]
        public void Handle_WhenApproveTransferRequest_AndSucceeds_ThenShouldUpdateCohortAndTransferRequestWithApprovalAction()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            _fixture
                .ApproveTransferRequest();

            // Assert
            _fixture.VerifyTransferRequestApprovalPropertiesAreSet();
        }

        [Test]
        public void Handle_WhenApproveTransferRequest_AndSucceeds_ThenShouldPublishTransferRequestApprovedEvent()
        {
            // Arange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            _fixture
                .ApproveTransferRequest();

            // Assert
            _fixture.VerifyTransferRequestApprovedEventIsPublished();
        }

        [Test]
        public void Handle_WhenApproveTransferRequest_ForApprovedTransferRequest_ThenShouldLogWarningAndReturn()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Approved);

            // Act
            _fixture.ApproveTransferRequest();

            // Assert
            _fixture
                .VerifyTransferRequestApprovedEventIsNotPublished();
            
            _fixture
                .VerifyHasWarning($"Transfer Request {_fixture.TransferRequest.Id} has already been approved");
        }

        [Test]
        public void Handle_WhenApproveTransferRequest_AndSucceeds_ThenShouldPublishChangeTrackingEvents()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            _fixture
                .ApproveTransferRequest();

            // Assert
            _fixture
                .VerifyEntityIsBeingTracked(UserAction.ApproveTransferRequest);
        }

        [Test]
        public void Handle_WhenApproveTransferRequest_AndFails_ThenShouldThrowAnExceptionAndLogIt()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            _fixture.ApproveTransferRequest(-1991);

            // Assert
            _fixture
                .VerifyHasError($"Error processing {nameof(ITransferRequestDomainService.ApproveTransferRequest)}");
        }

        [Test]
        public void WhenRejectTransferRequest_AndDbDoesNotHaveTransferRequest_ThenExceptionThrownAndLogged()
        {
            // Act
            Assert.ThrowsAsync<InvalidOperationException>(() => _fixture.RejectTransferRequest());

            // Assert
            _fixture.VerifyHasError($"Error processing {nameof(ITransferRequestDomainService.RejectTransferRequest)}");
        }

        [Test]
        public async Task WhenRejectTransferRequest_AndDbHasTransferRequest_ThenNoExceptionThrown()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            await _fixture.RejectTransferRequest();

            // Assert
            _fixture.VerifyHasNoError();
        }

        [TestCase(TransferApprovalStatus.Pending, false)]
        [TestCase(TransferApprovalStatus.Approved, true)]
        public async Task WhenRejectTransferRequest_ThenTransferApprovalStatusHandledCorrectly(TransferApprovalStatus status, bool shouldThrowException)
        {
            _fixture
                .WithTransferRequest(status);

            if (shouldThrowException)
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await _fixture.RejectTransferRequest());
            }
            else
            {
                await _fixture.RejectTransferRequest();
            }
        }

        [Test]
        public async Task WhenRejectTransferRequest_AndAlreadyBeenRejected_ThenLogWarning()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Rejected);

            // Act
            await _fixture.RejectTransferRequest();
            
            // Assert
            _fixture.VerifyHasWarning($"Transfer Request {_fixture.TransferRequest.Id} has already been rejected");
        }

        [Test]
        public async Task WhenRejectTransferRequest_ThenEntityIsTracked()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            await _fixture.RejectTransferRequest();

            // Assert
            _fixture.VerifyEntityIsBeingTracked(UserAction.RejectTransferRequest);
        }

        [Test]
        public async Task WhenRejectTransferRequest_ThenTransferRequestApprovalStatusUpdatedInDatabase()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            await _fixture.RejectTransferRequest();

            // Assert
            _fixture.VerifyTransferApprovalStatusUpdatedToRejected();
        }

        [Test]
        public async Task WhenRejectTransferRequest_ThenPublishesTransferRequestRejectedEvent()
        {
            // Arrange
            _fixture
                .WithTransferRequest(TransferApprovalStatus.Pending);

            // Act
            await _fixture.RejectTransferRequest();

            // Assert
            _fixture.VerifyTransferRequestRejectedEventIsPublished();
        }
    }

    public class TransferRequestDomainServiceTestsFixture
    {
        public DraftApprenticeship ExistingApprenticeshipDetails { get; set; }
        public UserInfo TransferSenderUserInfo { get; set; }
        public DateTime Now { get; }
        public TransferRequestDomainService Sut { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public Cohort Cohort { get; set; }
        public TransferRequest TransferRequest { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public Fixture Fixture { get; set; }
        public Mock<ILogger<TransferRequestDomainService>> Logger { get; set; }

        public TransferRequestDomainServiceTestsFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();
            Fixture = new Fixture();
            Now = DateTime.UtcNow;

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Logger = new Mock<ILogger<TransferRequestDomainService>>();
            Sut = new TransferRequestDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object);

            Cohort = new Cohort(
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                null,
                Party.Employer,
                "",
                new UserInfo())
            { EmployerAccountId = 100, TransferSenderId = 99 };

            ExistingApprenticeshipDetails = new DraftApprenticeship(Fixture.Build<DraftApprenticeshipDetails>().Create(), Party.Provider);
            Cohort.Apprenticeships.Add(ExistingApprenticeshipDetails);

            Cohort.EditStatus = EditStatus.Both;
            Cohort.TransferApprovalStatus = TransferApprovalStatus.Pending;
            Cohort.TransferSenderId = 10900;

            TransferSenderUserInfo = Fixture.Create<UserInfo>();
            TransferRequest = new TransferRequest
            { Status = TransferApprovalStatus.Pending, Cost = 1000, Cohort = Cohort };
        }

        public TransferRequestDomainServiceTestsFixture WithTransferRequest(TransferApprovalStatus status)
        {
            TransferRequest.Status = status;
            Db.TransferRequests.Add(TransferRequest);
            Db.SaveChanges();

            return this;
        }

        public Task ApproveTransferRequest(long transferRequestId = 0)
        {
            if (transferRequestId == 0)
            {
                transferRequestId = TransferRequest.Id;
            }

            return Sut.ApproveTransferRequest(transferRequestId, TransferSenderUserInfo, Now, default);
        }

        public void VerifyTransferRequestApprovalPropertiesAreSet()
        {
            Assert.AreEqual(TransferRequest.Status, TransferApprovalStatus.Approved);
            Assert.AreEqual(TransferRequest.TransferApprovalActionedOn, Now);
            Assert.AreEqual(TransferRequest.TransferApprovalActionedByEmployerName, TransferSenderUserInfo.UserDisplayName);
            Assert.AreEqual(TransferRequest.TransferApprovalActionedByEmployerEmail, TransferSenderUserInfo.UserEmail);
        }

        public void VerifyHasError(string expectedMessage)
        {
            Logger.VerifyLogging(expectedMessage, LogLevel.Error, Times.Once);
        }

        public void VerifyHasNoError()
        {
            Logger.VerifyLogging(LogLevel.Error, Times.Never);
        }

        public void VerifyHasWarning(string expectedMessage)
        {
            Logger.VerifyLogging(expectedMessage, LogLevel.Warning, Times.Once);
        }

        public void VerifyTransferRequestApprovedEventIsPublished()
        {
            var list = UnitOfWorkContext.GetEvents().OfType<TransferRequestApprovedEvent>().ToList();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(Cohort.Id, list[0].CohortId);
            Assert.AreEqual(TransferRequest.Id, list[0].TransferRequestId);
            Assert.AreEqual(TransferSenderUserInfo, list[0].UserInfo);
            Assert.AreEqual(Now, list[0].ApprovedOn);
        }

        public void VerifyTransferRequestApprovedEventIsNotPublished()
        {
            var list = UnitOfWorkContext.GetEvents().OfType<TransferRequestApprovedEvent>().ToList();

            Assert.AreEqual(0, list.Count);
        }

        public Task RejectTransferRequest(long transferRequestId = 0)
        {
            if (transferRequestId == 0)
            {
                transferRequestId = TransferRequest.Id;
            }

            return Sut.RejectTransferRequest(transferRequestId, TransferSenderUserInfo, Now, default);
        }

        public void VerifyTransferApprovalStatusUpdatedToRejected()
        {
            var transferRequest = Db.TransferRequests
                .SingleOrDefault(x => x.Id == TransferRequest.Id);

            if (transferRequest == null) Assert.Fail("TransferRequest not in database.");
            Assert.AreEqual(TransferApprovalStatus.Rejected, transferRequest.Status);
        }

        public void VerifyTransferRequestRejectedEventIsPublished()
        {
            var list = UnitOfWorkContext.GetEvents().OfType<TransferRequestRejectedEvent>().ToList();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(Cohort.Id, list[0].CohortId);
            Assert.AreEqual(TransferRequest.Id, list[0].TransferRequestId);
            Assert.AreEqual(TransferSenderUserInfo, list[0].UserInfo);
            Assert.AreEqual(Now, list[0].RejectedOn);
        }

        public void VerifyEntityIsBeingTracked(UserAction userAction)
        {
            var list = UnitOfWorkContext
                .GetEvents()
                .OfType<EntityStateChangedEvent>()
                .Where(x => x.StateChangeType == userAction).ToList();

            Assert.AreEqual(1, list.Count);

            Assert.AreEqual(userAction, list[0].StateChangeType);
            Assert.AreEqual(TransferRequest.Id, list[0].EntityId);
            Assert.AreEqual(TransferSenderUserInfo.UserId, list[0].UpdatingUserId);
            Assert.AreEqual(TransferSenderUserInfo.UserDisplayName, list[0].UpdatingUserName);
            Assert.AreEqual(Party.TransferSender, list[0].UpdatingParty);
        }

        public void TearDown()
        {
            Db.Database.EnsureDeleted();
        }
    }
}