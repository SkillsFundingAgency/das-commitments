using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ProcessFullyApprovedCohortCommandHandlerTests
    {
        private ProcessFullyApprovedCohortCommandFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ProcessFullyApprovedCohortCommandFixture();
        }

        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        [TestCase(ApprenticeshipEmployerType.Levy)]
        public void Handle_WhenHandlingCommand_ThenShouldProcessFullyApprovedCohort(ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            _fixture.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
                .Handle();
            
            _fixture.Db.Verify(d => d.ExecuteSqlCommandAsync(
                    "EXEC ProcessFullyApprovedCohort @cohortId, @accountId, @apprenticeshipEmployerType",
                    It.Is<SqlParameter>(p => p.ParameterName == "cohortId" && p.Value.Equals(_fixture.Command.CohortId)),
                    It.Is<SqlParameter>(p => p.ParameterName == "accountId" && p.Value.Equals(_fixture.Command.AccountId)),
                    It.Is<SqlParameter>(p => p.ParameterName == "apprenticeshipEmployerType" && p.Value.Equals(apprenticeshipEmployerType))),
                Times.Once);
        }
        
        [TestCase(ApprenticeshipEmployerType.NonLevy, false)]
        [TestCase(ApprenticeshipEmployerType.NonLevy, true)]
        [TestCase(ApprenticeshipEmployerType.Levy, false)]
        [TestCase(ApprenticeshipEmployerType.Levy, true)]
        public void Handle_WhenHandlingCommand_ThenShouldPublishEvents(ApprenticeshipEmployerType apprenticeshipEmployerType, bool isFundedByTransfer)
        {
            _fixture.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
                .SetApprovedApprenticeships(isFundedByTransfer)
                .Handle();
            
            _fixture.ApprovedApprenticeships.ForEach(
                a => _fixture.EventPublisher.Verify(
                    p => p.Publish(It.Is<ApprenticeshipCreatedEvent>(
                        e => _fixture.IsValid(apprenticeshipEmployerType, a, e))),
                    Times.Once));
        }
    }

    public class ProcessFullyApprovedCohortCommandFixture
    {
        public IFixture AutoFixture { get; set; }
        public ProcessFullyApprovedCohortCommand Command { get; set; }
        public Mock<IAccountApiClient> AccountApiClient { get; set; }
        public Mock<ProviderCommitmentsDbContext> Db { get; set; }
        public Mock<IEventPublisher> EventPublisher { get; set; }
        public List<ApprovedApprenticeship> ApprovedApprenticeships { get; set; }
        public IRequestHandler<ProcessFullyApprovedCohortCommand> Handler { get; set; }
        
        public ProcessFullyApprovedCohortCommandFixture()
        {
            AutoFixture = new Fixture();
            Command = AutoFixture.Create<ProcessFullyApprovedCohortCommand>();
            AccountApiClient = new Mock<IAccountApiClient>();
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            EventPublisher = new Mock<IEventPublisher>();
            ApprovedApprenticeships = new List<ApprovedApprenticeship>();
            Handler = new ProcessFullyApprovedCohortCommandHandler(AccountApiClient.Object, new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), EventPublisher.Object);
            
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            Db.Setup(d => d.ExecuteSqlCommandAsync(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task.CompletedTask);
            EventPublisher.Setup(p => p.Publish(It.IsAny<object>())).Returns(Task.CompletedTask);
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken.None);
        }

        public ProcessFullyApprovedCohortCommandFixture SetApprenticeshipEmployerType(ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            AccountApiClient.Setup(c => c.GetAccount(Command.AccountId))
                .ReturnsAsync(new AccountDetailViewModel
                {
                    ApprenticeshipEmployerType = apprenticeshipEmployerType.ToString()
                });
            
            return this;
        }

        public ProcessFullyApprovedCohortCommandFixture SetApprovedApprenticeships(bool isFundedByTransfer)
        {
            var cohortBuilder = AutoFixture.Build<Cohort>().Without(c => c.Apprenticeships);

            if (!isFundedByTransfer)
            {
                cohortBuilder.Without(c => c.TransferSenderId).Without(c => c.TransferApprovalActionedOn);
            }
            
            var approvedApprenticeshipBuilder = AutoFixture.Build<ApprovedApprenticeship>().Without(a => a.DataLockStatus).Without(a => a.EpaOrg);
            var cohort1 = cohortBuilder.With(c => c.Id, Command.CohortId).Create();
            var cohort2 = cohortBuilder.Create();
            var approvedApprenticeship1 = approvedApprenticeshipBuilder.With(a => a.Cohort, cohort1).Create();
            var approvedApprenticeship2 = approvedApprenticeshipBuilder.With(a => a.Cohort, cohort1).Create();
            var approvedApprenticeship3 = approvedApprenticeshipBuilder.With(a => a.Cohort, cohort2).Create();
            var approvedApprenticeships1 = new[] { approvedApprenticeship1, approvedApprenticeship2 };
            var approvedApprenticeships2 = new[] { approvedApprenticeship1, approvedApprenticeship2, approvedApprenticeship3 };
            
            ApprovedApprenticeships.AddRange(approvedApprenticeships1);
            Db.Object.ApprovedApprenticeships.AddRange(approvedApprenticeships2);
            Db.Object.SaveChanges();
            
            return this;
        }

        public bool IsValid(ApprenticeshipEmployerType apprenticeshipEmployerType, ApprovedApprenticeship approvedApprenticeship, ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            var isValid = apprenticeshipCreatedEvent.ApprenticeshipId == approvedApprenticeship.Id &&
                          apprenticeshipCreatedEvent.CreatedOn == (approvedApprenticeship.Cohort.TransferApprovalActionedOn ?? approvedApprenticeship.AgreedOn.Value) &&
                          apprenticeshipCreatedEvent.AgreedOn == approvedApprenticeship.AgreedOn.Value &&
                          apprenticeshipCreatedEvent.AccountId == approvedApprenticeship.Cohort.EmployerAccountId &&
                          apprenticeshipCreatedEvent.AccountLegalEntityPublicHashedId == approvedApprenticeship.Cohort.AccountLegalEntityPublicHashedId &&
                          apprenticeshipCreatedEvent.LegalEntityName == approvedApprenticeship.Cohort.LegalEntityName &&
                          apprenticeshipCreatedEvent.ProviderId == approvedApprenticeship.Cohort.ProviderId.Value &&
                          apprenticeshipCreatedEvent.TransferSenderId == approvedApprenticeship.Cohort.TransferSenderId &&
                          apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval == apprenticeshipEmployerType &&
                          apprenticeshipCreatedEvent.Uln == approvedApprenticeship.Uln &&
                          apprenticeshipCreatedEvent.TrainingType == approvedApprenticeship.ProgrammeType.Value &&
                          apprenticeshipCreatedEvent.TrainingCode == approvedApprenticeship.CourseCode &&
                          apprenticeshipCreatedEvent.StartDate == approvedApprenticeship.StartDate.Value &&
                          apprenticeshipCreatedEvent.EndDate == approvedApprenticeship.EndDate.Value &&
                          apprenticeshipCreatedEvent.PriceEpisodes.Count() == approvedApprenticeship.PriceHistory.Count;

            for (var i = 0; i < approvedApprenticeship.PriceHistory.Count; i++)
            {
                var priceHistory = approvedApprenticeship.PriceHistory.ElementAt(i);
                var priceEpisode = apprenticeshipCreatedEvent.PriceEpisodes.ElementAtOrDefault(i);

                isValid = isValid &&
                          priceEpisode?.FromDate == priceHistory.FromDate &
                          priceEpisode?.ToDate == priceHistory.ToDate &
                          priceEpisode?.Cost == priceHistory.Cost;
            }
            
            return isValid;
        }
    }
}