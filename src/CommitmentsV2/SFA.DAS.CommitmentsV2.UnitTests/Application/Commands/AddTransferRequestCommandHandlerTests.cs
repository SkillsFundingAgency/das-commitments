using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.SystemFunctions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddTransferRequestCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldCreateTransferRequest()
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertTransferRequestWasCorrectlySavedToDatabase();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldSetCohortTransferStatus()
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertCohortTransferStatusIsSetToPending();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldPublishTransferRequestCreatedEvent()
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();

            fixture.AssertTransferRequestCreatedEventWasPublished();
        }

        [Test]
        public void Handle_WhenCommandIsHandledAndPendingTransferRequestExists_ThenShouldThrowDomainException()
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort().SetupPendingTransferRequest();
            Assert.ThrowsAsync<DomainException>(() => fixture.Handle());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Handle_WhenCommandIsHandledAndCohortIsLinkedToPledgeApplication_ThenAutoApprovalFlagIsDeterminedFromApplication(bool autoApproval)
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort()
                .SetupPledgeApplication(autoApproval);

            await fixture.Handle();

            fixture.AssertTransferRequestAutoApprovalEquals(autoApproval);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandledAndCohortIsNotLinkedToPledgeApplication_ThenAutoApprovalFlagIsFalse()
        {
            using var fixture = new AddTransferRequestCommandHandlerTestFixture().SetupCohort();
            await fixture.Handle();
            fixture.AssertTransferRequestAutoApprovalEquals(false);
        }
    }

    public class AddTransferRequestCommandHandlerTestFixture : IDisposable
    {
        public Fixture Fixture { get; set; }
        public long CohortId { get; set; }
        public Cohort Cohort { get; set; }
        public FundingCapCourseSummary FundingCapCourseSummary1 { get; set; }
        public FundingCapCourseSummary FundingCapCourseSummary2 { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public Mock<IFundingCapService> FundingService { get; set; }
        public Mock<IApprovalsOuterApiClient> LevyTransferMatchingApiClient { get; set; }
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

            LevyTransferMatchingApiClient = new Mock<IApprovalsOuterApiClient>();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            Handler = new AddTransferRequestCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                FundingService.Object,
                Mock.Of<ILogger<AddTransferRequestCommandHandler>>(),
                LevyTransferMatchingApiClient.Object);

            UnitOfWorkContext = new UnitOfWorkContext();
            LastApprovedByParty = Party.Employer;
        }

        public AddTransferRequestCommandHandlerTestFixture SetupCohort()
        {
            Cohort = new Cohort(
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                Fixture.Create<long>(),
                null,
                null,
                Party.Employer,
                "",
                new UserInfo());

            Db.Cohorts.Add(Cohort);
            Db.SaveChanges();

            CohortId = Cohort.Id;

            return this;
        }

        public AddTransferRequestCommandHandlerTestFixture SetupPendingTransferRequest()
        {
            Db.TransferRequests.Add(new TransferRequest
                { CommitmentId = CohortId, Status = (byte)TransferApprovalStatus.Pending, });

            return this;
        }

        public AddTransferRequestCommandHandlerTestFixture SetupPledgeApplication(bool autoApproval)
        {
            Cohort.PledgeApplicationId = Fixture.Create<int>();

            LevyTransferMatchingApiClient.Setup(x => x.Get<PledgeApplication>(It.IsAny<GetPledgeApplicationRequest>()))
                .ReturnsAsync(new PledgeApplication { AutomaticApproval = autoApproval});

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
            Assert.That(transferRequest.TrainingCourses, Is.Not.Null);
            Assert.That(transferRequest.TrainingCourses.IndexOf(FundingCapCourseSummary1.CourseTitle) >= 0, Is.True);
            Assert.That(transferRequest.TrainingCourses.IndexOf(FundingCapCourseSummary2.CourseTitle) >= 0, Is.True);
            Assert.That(transferRequest.FundingCap, Is.EqualTo(FundingCapCourseSummary1.ActualCap + FundingCapCourseSummary2.ActualCap));
            Assert.That(transferRequest.Cost, Is.EqualTo(FundingCapCourseSummary1.CappedCost + FundingCapCourseSummary2.CappedCost));
        }

        public void AssertCohortTransferStatusIsSetToPending()
        {
            var cohort = Db.Cohorts.First();
            Assert.That(cohort.TransferApprovalStatus, Is.EqualTo(TransferApprovalStatus.Pending));
        }

        public void AssertTransferRequestCreatedEventWasPublished()
        {
            var @event = UnitOfWorkContext.GetEvents().OfType<TransferRequestCreatedEvent>().First();
            Assert.That(@event.CohortId, Is.EqualTo(CohortId));
            Assert.That(@event.LastApprovedByParty, Is.EqualTo(LastApprovedByParty));
        }

        public void AssertTransferRequestAutoApprovalEquals(bool autoApproval)
        {
            var transferRequest = Db.TransferRequests.FirstOrDefault();
            Assert.That(transferRequest.AutoApproval, Is.EqualTo(autoApproval));
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}