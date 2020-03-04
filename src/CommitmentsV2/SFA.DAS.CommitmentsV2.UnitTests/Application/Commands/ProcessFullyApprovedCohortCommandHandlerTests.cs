using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        [TestCase(ApprenticeshipEmployerType.Levy)]
        public void Handle_WhenHandlingCommand_ThenShouldProcessFullyApprovedCohort(ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            var f = new ProcessFullyApprovedCohortCommandFixture();
            f.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
                .Handle();
            
            f.Db.Verify(d => d.ExecuteSqlCommandAsync(
                    "EXEC ProcessFullyApprovedCohort @cohortId, @accountId, @apprenticeshipEmployerType",
                    It.Is<SqlParameter>(p => p.ParameterName == "cohortId" && p.Value.Equals(f.Command.CohortId)),
                    It.Is<SqlParameter>(p => p.ParameterName == "accountId" && p.Value.Equals(f.Command.AccountId)),
                    It.Is<SqlParameter>(p => p.ParameterName == "apprenticeshipEmployerType" && p.Value.Equals(apprenticeshipEmployerType))),
                Times.Once);
        }
        
        [TestCase(ApprenticeshipEmployerType.NonLevy, false)]
        [TestCase(ApprenticeshipEmployerType.NonLevy, true)]
        [TestCase(ApprenticeshipEmployerType.Levy, false)]
        [TestCase(ApprenticeshipEmployerType.Levy, true)]
        public void Handle_WhenHandlingCommand_ThenShouldPublishEvents(ApprenticeshipEmployerType apprenticeshipEmployerType, bool isFundedByTransfer)
        {
            var f = new ProcessFullyApprovedCohortCommandFixture();
            f.SetApprenticeshipEmployerType(apprenticeshipEmployerType)
                .SetApprovedApprenticeships(isFundedByTransfer)
                .Handle();
            
            f.Apprenticeships.ForEach(
                a => f.EventPublisher.Verify(
                    p => p.Publish(It.Is<ApprenticeshipCreatedEvent>(
                        e => f.IsValid(apprenticeshipEmployerType, a, e))),
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
        public List<Apprenticeship> Apprenticeships { get; set; }
        public IRequestHandler<ProcessFullyApprovedCohortCommand> Handler { get; set; }
        
        public ProcessFullyApprovedCohortCommandFixture()
        {
            AutoFixture = new Fixture();
            Command = AutoFixture.Create<ProcessFullyApprovedCohortCommand>();
            AccountApiClient = new Mock<IAccountApiClient>();
            Db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            EventPublisher = new Mock<IEventPublisher>();
            Apprenticeships = new List<Apprenticeship>();
            Handler = new ProcessFullyApprovedCohortCommandHandler(AccountApiClient.Object, new Lazy<ProviderCommitmentsDbContext>(() => Db.Object), EventPublisher.Object, Mock.Of<ILogger<ProcessFullyApprovedCohortCommandHandler>>());
            
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
            var provider = new Provider {Name = "Test Provider"};
            var account = new Account(1, "", "", "", DateTime.UtcNow);
            var accountLegalEntity = new AccountLegalEntity(account, 1, 1, "", "", "Test Employer", OrganisationType.Charities, "", DateTime.UtcNow);

            var cohortBuilder = AutoFixture.Build<Cohort>()
                .Without(c => c.Apprenticeships)
                .With(c => c.AccountLegalEntity, accountLegalEntity)
                .With(c => c.Provider, provider)
                .With(x => x.IsDeleted, false);

            if (!isFundedByTransfer)
            {
                cohortBuilder.Without(c => c.TransferSenderId).Without(c => c.TransferApprovalActionedOn);
            }
            
            var apprenticeshipBuilder = AutoFixture.Build<Apprenticeship>().Without(a => a.DataLockStatus).Without(a => a.EpaOrg).Without(a => a.ApprenticeshipUpdate);
            var cohort1 = cohortBuilder.With(c => c.Id, Command.CohortId).Create();
            var cohort2 = cohortBuilder.Create();
            var apprenticeship1 = apprenticeshipBuilder.With(a => a.Cohort, cohort1).Create();
            var apprenticeship2 = apprenticeshipBuilder.With(a => a.Cohort, cohort1).Create();
            var apprenticeship3 = apprenticeshipBuilder.With(a => a.Cohort, cohort2).Create();
            var apprenticeships1 = new[] { apprenticeship1, apprenticeship2 };
            var apprenticeships2 = new[] { apprenticeship1, apprenticeship2, apprenticeship3 };
            
            Apprenticeships.AddRange(apprenticeships1);
            Db.Object.AccountLegalEntities.Add(accountLegalEntity);
            Db.Object.Providers.Add(provider);
            Db.Object.Apprenticeships.AddRange(apprenticeships2);
            Db.Object.SaveChanges();
            
            return this;
        }

        public bool IsValid(ApprenticeshipEmployerType apprenticeshipEmployerType, Apprenticeship apprenticeship, ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            var isValid = apprenticeshipCreatedEvent.ApprenticeshipId == apprenticeship.Id &&
                          apprenticeshipCreatedEvent.CreatedOn == (apprenticeship.Cohort.TransferApprovalActionedOn ?? apprenticeship.AgreedOn.Value) &&
                          apprenticeshipCreatedEvent.AgreedOn == apprenticeship.AgreedOn.Value &&
                          apprenticeshipCreatedEvent.AccountId == apprenticeship.Cohort.EmployerAccountId &&
                          apprenticeshipCreatedEvent.AccountLegalEntityPublicHashedId == apprenticeship.Cohort.AccountLegalEntityPublicHashedId &&
                          apprenticeshipCreatedEvent.LegalEntityName == apprenticeship.Cohort.AccountLegalEntity.Name &&
                          apprenticeshipCreatedEvent.ProviderId == apprenticeship.Cohort.Provider.UkPrn &&
                          apprenticeshipCreatedEvent.TransferSenderId == apprenticeship.Cohort.TransferSenderId &&
                          apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval == apprenticeshipEmployerType &&
                          apprenticeshipCreatedEvent.Uln == apprenticeship.Uln &&
                          apprenticeshipCreatedEvent.TrainingType == apprenticeship.ProgrammeType.Value &&
                          apprenticeshipCreatedEvent.TrainingCode == apprenticeship.CourseCode &&
                          apprenticeshipCreatedEvent.StartDate == apprenticeship.StartDate.Value &&
                          apprenticeshipCreatedEvent.EndDate == apprenticeship.EndDate.Value &&
                          apprenticeshipCreatedEvent.PriceEpisodes.Count() == apprenticeship.PriceHistory.Count;

            for (var i = 0; i < apprenticeship.PriceHistory.Count; i++)
            {
                var priceHistory = apprenticeship.PriceHistory.ElementAt(i);
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