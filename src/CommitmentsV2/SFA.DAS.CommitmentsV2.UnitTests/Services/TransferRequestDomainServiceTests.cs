using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenCorrectEmployerTransferRequestPendingNotificationsReturned(List<DataCases.Input> inputs, List<EmployerTransferRequestPendingNotification> expectedResults)
        {
            // Arrange
            foreach (var input in inputs)
            {
                _fixture
                    .WithTransferRequest(input.TransferRequestInput.AccountLegalEntityId, input.TransferRequestInput.LegalEntityId,
                    input.TransferRequestInput.LegalEntityName, input.TransferRequestInput.AccountId, input.TransferRequestInput.CohortId,
                    input.TransferRequestInput.CohortReference, input.TransferRequestInput.TransferSenderId, input.TransferRequestInput.TransferRequestId, 
                    input.TransferRequestInput.Status, input.TransferRequestInput.AutoApproval);
            }

            // Act
            var results = await _fixture.GetEmployerTransferRequestPendingNotifications();

            // Assert
            results.Should().BeEquivalentTo(expectedResults);
        }

        [Test]
        public async Task GetTransferRequestSummary()
        {
            // Arrange
            _fixture.WithTransferRequestSummary();

            // Act
            await _fixture.GetTransferRequestSummary();

            // Assert
            _fixture.VerifyTransferRequestSummary();
        }
    }

    public class DataCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            #region no notifications
            yield return new object[]
            {
                    new List<Input>
                    {
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", LegalEntityName = "NAME1", AccountId = 1001, 
                                CohortId = 1,  CohortReference = "REF1", TransferSenderId = 11, TransferRequestId = 1, Status = TransferApprovalStatus.Approved, AutoApproval = false }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 2, LegalEntityId = "LE2", LegalEntityName = "NAME2", AccountId = 1002, 
                                CohortId = 2,  CohortReference = "REF2", TransferSenderId = 22, TransferRequestId = 2, Status = TransferApprovalStatus.Approved, AutoApproval = true }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 3, LegalEntityId = "LE3", LegalEntityName = "NAME3", AccountId = 1003, 
                                CohortId = 3,  CohortReference = "REF3", TransferSenderId = 33, TransferRequestId = 3, Status = TransferApprovalStatus.Rejected, AutoApproval = false }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 4, LegalEntityId = "LE4", LegalEntityName = "NAME4", AccountId = 1004, 
                                CohortId = 4,  CohortReference = "REF4", TransferSenderId = 44, TransferRequestId = 4, Status = TransferApprovalStatus.Rejected, AutoApproval = true }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 5, LegalEntityId = "LE5", LegalEntityName = "NAME5", AccountId = 1005, 
                                CohortId = 5,  CohortReference = "REF5", TransferSenderId = 55, TransferRequestId = 5, Status = TransferApprovalStatus.Pending, AutoApproval = true }
                        }
                    },
                    new List<EmployerTransferRequestPendingNotification>
                    {
                    }
            };
            #endregion

            #region multiple notifications
            yield return new object[]
            {
                   new List<Input>
                   {
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", LegalEntityName = "NAME1", AccountId = 1001, 
                                CohortId = 1, CohortReference = "REF1", TransferSenderId = 11, TransferRequestId = 1, Status = TransferApprovalStatus.Approved, AutoApproval = false }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 2, LegalEntityId = "LE2", LegalEntityName = "NAME2", AccountId = 1002, 
                                CohortId = 2, CohortReference = "REF2", TransferSenderId = 22, TransferRequestId = 2, Status = TransferApprovalStatus.Rejected, AutoApproval = false }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 3, LegalEntityId = "LE3", LegalEntityName = "NAME3", AccountId = 1003, 
                                CohortId = 3, CohortReference = "REF3", TransferSenderId = 33, TransferRequestId = 3, Status = TransferApprovalStatus.Pending, AutoApproval = false }
                        },
                        new Input
                        {
                            TransferRequestInput =  new TransferRequestInput { AccountLegalEntityId = 4, LegalEntityId = "LE4", LegalEntityName = "NAME4", AccountId = 1004, 
                                CohortId = 4, CohortReference = "REF4", TransferSenderId = 44, TransferRequestId = 4, Status = TransferApprovalStatus.Pending, AutoApproval = false }
                        }
                    },
                    new List<EmployerTransferRequestPendingNotification>
                    {
                        new EmployerTransferRequestPendingNotification
                        {
                            TransferRequestId = 3, ReceivingEmployerAccountId = 1003, ReceivingLegalEntityName = "NAME3", CohortReference = "REF3", SendingEmployerAccountId = 33, 
                            CommitmentId = 3, Status = TransferApprovalStatus.Pending
                        },
                        new EmployerTransferRequestPendingNotification
                        {
                            TransferRequestId = 4, ReceivingEmployerAccountId = 1004, ReceivingLegalEntityName = "NAME4", CohortReference = "REF4", SendingEmployerAccountId = 44,
                            CommitmentId = 4, Status = TransferApprovalStatus.Pending
                        }
                    }
            };
            #endregion
        }

        #region Test Data Classes
        public class Input
        {
            public TransferRequestInput TransferRequestInput { get; set; }
        }

        public class TransferRequestInput
        {
            public long AccountLegalEntityId { get; set; }
            public string LegalEntityId { get; set; }
            public string LegalEntityName { get; set; }
            public long AccountId { get; set; }
            public long CohortId { get; set; }
            public string CohortReference { get; set; }
            public long TransferSenderId { get; set; }
            public long TransferRequestId { get; internal set; }
            public TransferApprovalStatus Status { get; set; }
            public bool AutoApproval { get; set; }
        }
        #endregion
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
        public GetTransferRequestsSummaryQueryResult getTransferRequestsSummaryQueryResult { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public Fixture Fixture { get; set; }
        public Mock<ILogger<TransferRequestDomainService>> Logger { get; set; }

        public List<TransferRequest> SeedTransferRequests { get; }

        public TransferRequestDomainServiceTestsFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();
            Fixture = new Fixture();
            Now = DateTime.UtcNow;

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            SeedTransferRequests = new List<TransferRequest>();

            Logger = new Mock<ILogger<TransferRequestDomainService>>();
            Sut = new TransferRequestDomainService(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object);

            var accountLegalEntity = new AccountLegalEntity()
             .Set(a => a.LegalEntityId, Fixture.Create<string>())
             .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
             .Set(a => a.Id, 444);

            Cohort = new Cohort(
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                null,
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
            Cohort.AccountLegalEntity = accountLegalEntity;
            Cohort.EmployerAccountId = 222;
            
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

        public TransferRequestDomainServiceTestsFixture WithTransferRequest(long accountLegalEntityId, string legalEntityId, string legalEntityName, long accountId, 
            long cohortId, string cohortReference, long transferSenderId, long transferRequestId, TransferApprovalStatus transferApprovalStatus, bool autoApproval)
        {
            var accountLegalEntity =
                SeedTransferRequests.FirstOrDefault(p => p.Cohort.AccountLegalEntity.Id == accountLegalEntityId)?.Cohort?.AccountLegalEntity
                    ?? new AccountLegalEntity()
                        .Set(a => a.LegalEntityId, legalEntityId)
                        .Set(a => a.Name, legalEntityName)
                        .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
                        .Set(a => a.AccountId, accountId)
                        .Set(a => a.Id, accountLegalEntityId);

            var cohort =
                SeedTransferRequests.FirstOrDefault(p => p.Cohort.Id == cohortId)?.Cohort
                    ?? new Cohort()
                        .Set(c => c.Id, cohortId)
                        .Set(c => c.TransferSenderId, transferSenderId)
                        .Set(c => c.Reference, cohortReference)
                        .Set(c => c.EmployerAccountId, accountId)
                        .Set(c => c.AccountLegalEntity, accountLegalEntity)
                        .Set(c => c.AccountLegalEntityId, accountLegalEntity.Id);

            var transferRequest = new TransferRequest()
                .Set(s => s.Id, transferRequestId)
                .Set(s => s.CommitmentId, cohort.Id)
                .Set(s => s.Cohort, cohort)
                .Set(s => s.Status, transferApprovalStatus)
                .Set(s => s.AutoApproval, autoApproval);

            SeedTransferRequests.Add(transferRequest);
            return this;
        }

        public TransferRequestDomainServiceTestsFixture WithTransferRequestSummary()
        {
            TransferRequest.Status = TransferApprovalStatus.Pending;
            TransferRequest.FundingCap = 2;
            TransferRequest.CreatedOn = DateTime.UtcNow;
            TransferRequest.TransferApprovalActionedOn = DateTime.UtcNow;
            TransferRequest.TransferApprovalActionedByEmployerName = TransferSenderUserInfo.UserDisplayName;
            TransferRequest.TransferApprovalActionedByEmployerEmail = TransferSenderUserInfo.UserEmail;             
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

        public Task GetTransferRequestSummary()
        {
            return Sut.GetTransferRequestSummary(222, default);
        }

        public Task<List<EmployerTransferRequestPendingNotification>> GetEmployerTransferRequestPendingNotifications()
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var service = new TransferRequestDomainService(lazy, Mock.Of<ILogger<TransferRequestDomainService>>());

                return service.GetEmployerTransferRequestPendingNotifications();
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            using (var dbContext = new ProviderCommitmentsDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            }
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.TransferRequests.AddRange(SeedTransferRequests);
            dbContext.SaveChanges(true);
        }

        public void VerifyTransferRequestSummary()
        {
            var transferSummary = Db.TransferRequests.Where(x => x.Cohort.EmployerAccountId == 222).First();
            if (transferSummary == null) Assert.Fail("TransferRequest not in database.");
            Assert.AreEqual(transferSummary.Cost, 1000);
            Assert.AreEqual(transferSummary.Status, TransferApprovalStatus.Pending);
            Assert.AreEqual(transferSummary.TransferApprovalActionedByEmployerName, TransferSenderUserInfo.UserDisplayName);
            Assert.AreEqual(transferSummary.TransferApprovalActionedByEmployerEmail, TransferSenderUserInfo.UserEmail);
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