using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddTransferRequestCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldCreateTransferRequest()
        {
            var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertTransferRequestWasCorrectlySavedToDatabase();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldSetCohortTransferStatus()
        {
            var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertCohortTransferStatusIsSetToPending();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldPublishTransferRequestCreatedEvent()
        {
            var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertTransferRequestCreatedEventWasPublished();
        }

        [Test]
        public void Handle_WhenCommandIsHandledAndPendingTransferRequestExists_ThenShouldThrowDomainException()
        {
            var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort().SetupPendingTransferRequest();
            Assert.ThrowsAsync<DomainException>(() => fixture.Handle());
        }
    }

    public class AddTransferRequestCommandHandlerTestFixture
    {
        public Fixture Fixture { get; set; }
        public long CohortId { get; set; }
        public FundingCapCourseSummary FundingCapCourseSummary1 { get; set; }
        public FundingCapCourseSummary FundingCapCourseSummary2 { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public Mock<IFundingCapService> FundingService { get; set; }
        public IRequestHandler<AddTransferRequestCommand> Handler { get; set; }

        public Party LastApprovedByParty { get; set; }

        public AddTransferRequestCommandHandlerTestFixture()
        {
            Fixture = new Fixture();
            CancellationToken = new CancellationToken();

            FundingCapCourseSummary1 = new FundingCapCourseSummary
                {ActualCap = 1000, ApprenticeshipCount = 1, CappedCost = 1200, CourseTitle = "C1Title"};
            FundingCapCourseSummary2 = new FundingCapCourseSummary
                {ActualCap = 1100, ApprenticeshipCount = 2, CappedCost = 1300, CourseTitle = "C2Title"};
            FundingService = new Mock<IFundingCapService>();
            FundingService.Setup(x => x.FundingCourseSummary(It.IsAny<IEnumerable<ApprenticeshipBase>>()))
                .ReturnsAsync(new List<FundingCapCourseSummary> {FundingCapCourseSummary1, FundingCapCourseSummary2});

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Handler = new AddTransferRequestCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                FundingService.Object,
                Mock.Of<ILogger<AddTransferRequestCommandHandler>>());

            UnitOfWorkContext = new UnitOfWorkContext();
            LastApprovedByParty = Party.Employer;
        }

        public AddTransferRequestCommandHandlerTestFixture SetupCohort()
        {
            var cohort = new Cohort(
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                null,
                null,
                Party.Employer,
                "",
                new UserInfo());

            Db.Cohorts.Add(cohort);
            Db.SaveChanges();

            CohortId = cohort.Id;

            return this;
        }

        public AddTransferRequestCommandHandlerTestFixture SetupPendingTransferRequest()
        {
            Db.TransferRequests.Add(new TransferRequest
                { CommitmentId = CohortId, Status = (byte)TransferApprovalStatus.Pending, });

            return this;
        }

        public async Task Handle()
        {
            var cmd = new AddTransferRequestCommand { CohortId = this.CohortId, LastApprovedByParty = LastApprovedByParty};
            await Handler.Handle(cmd, CancellationToken);
        }

        public void AssertTransferRequestWasCorrectlySavedToDatabase()
        {
            var transferRequest = Db.TransferRequests.FirstOrDefault();
            Assert.IsNotNull(transferRequest.TrainingCourses);
            Assert.IsTrue(transferRequest.TrainingCourses.IndexOf(FundingCapCourseSummary1.CourseTitle) >= 0);
            Assert.IsTrue(transferRequest.TrainingCourses.IndexOf(FundingCapCourseSummary2.CourseTitle) >= 0);
            Assert.AreEqual(FundingCapCourseSummary1.ActualCap + FundingCapCourseSummary2.ActualCap, transferRequest.FundingCap);
            Assert.AreEqual(FundingCapCourseSummary1.CappedCost + FundingCapCourseSummary2.CappedCost, transferRequest.Cost);
        }

        public void AssertCohortTransferStatusIsSetToPending()
        {
            var cohort = Db.Cohorts.First();
            Assert.AreEqual(TransferApprovalStatus.Pending, cohort.TransferApprovalStatus);
        }

        public void AssertTransferRequestCreatedEventWasPublished()
        {
            var @event = UnitOfWorkContext.GetEvents().OfType<TransferRequestCreatedEvent>().First();
            Assert.AreEqual(CohortId, @event.CohortId);
            Assert.AreEqual(LastApprovedByParty, @event.LastApprovedByParty);
            Assert.IsNotNull(@event.TransferRequestId);
        }
    }
}