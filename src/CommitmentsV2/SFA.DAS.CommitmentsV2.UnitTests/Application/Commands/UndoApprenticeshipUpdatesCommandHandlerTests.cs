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
using SFA.DAS.CommitmentsV2.Application.Commands.UndoApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UndoApprenticeshipUpdatesCommandHandlerTests 
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_PendingOriginatorIsNULL()
        {
            using var fixture = new UndoApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.That(fixture.ApprenticeshipFromDb.PendingUpdateOriginator, Is.EqualTo(null));
            
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdateStatus_IsUndone()
        {
            using var fixture = new UndoApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.That(fixture.ApprenticeshipUpdate.Status, Is.EqualTo(ApprenticeshipUpdateStatus.Deleted));
        }


        [Test]
        public async Task Handle_WhenNoApprenticeshipUpdate_AndCommandIsHandled_ExceptionIsThrown()
        {
            using var fixture = new UndoApprenticeshipUpdatesCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyException<InvalidOperationException>();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdateCancelledEvent_IsEmitted()
        {
            using var fixture = new UndoApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            var list = fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdateCancelledEvent>().ToList();

            var apprenticeship = fixture.ApprenticeshipFromDb;
            var priceEpisode = apprenticeship.PriceHistory.Select(x => new PriceEpisode
            {
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                Cost = x.Cost
            }).ToArray();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].ApprenticeshipId, Is.EqualTo(apprenticeship.Id));
            Assert.That(list[0].AccountId, Is.EqualTo(apprenticeship.Cohort.EmployerAccountId));
            Assert.That(list[0].ProviderId, Is.EqualTo(apprenticeship.Cohort.ProviderId));
        }
    }

    public class UndoApprenticeshipUpdatesCommandHandlerTestsFixture : IDisposable
    {
        public long ApprenticeshipId = 12;
        public Fixture fixture { get; set; }
        public UndoApprenticeshipUpdatesCommand Command { get; set; }
        public Apprenticeship ApprenticeshipDetails { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<UndoApprenticeshipUpdatesCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public Mock<IAuthenticationService> AuthenticationService;
        public Mock<ICurrentDateTime> currentDateTimeService;
        public Mock<IOverlapCheckService> OverlapCheckService;
        public Party Party;
        public bool HasOverlapErrors;
        public ApprenticeshipUpdate ApprenticeshipUpdate;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public Apprenticeship ApprenticeshipFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);

        public Exception Exception { get; set; }

        public DateTime proxyCurrentDateTime = new DateTime(2020, 1, 1);

        public UndoApprenticeshipUpdatesCommandHandlerTestsFixture()
        {
            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Party = Party.Employer;
            HasOverlapErrors = false;
            UnitOfWorkContext = new UnitOfWorkContext();

            var Cohort = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

            ApprenticeshipUpdate = new ApprenticeshipUpdate()
                .Set(c => c.ApprenticeshipId, ApprenticeshipId)
                .Set(c => c.Status, ApprenticeshipUpdateStatus.Pending); 

            var priceHistory = new List<PriceHistory>()
            {
                new PriceHistory
                {
                    FromDate = DateTime.Now,
                    ToDate = null,
                    Cost = 10000,
                }
            };

            ApprenticeshipDetails = fixture.Build<Apprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, Cohort)
             .With(s => s.PaymentStatus, PaymentStatus.Completed)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .With(s => s.PriceHistory, priceHistory)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);
            
            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), ApprenticeshipId, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new OverlapCheckResult(HasOverlapErrors, HasOverlapErrors)));

            currentDateTimeService = new Mock<ICurrentDateTime>();
            currentDateTimeService.Setup(x => x.UtcNow).Returns(proxyCurrentDateTime);

            UserInfo = fixture.Create<UserInfo>();
            Command = fixture.Build<UndoApprenticeshipUpdatesCommand>().With(o => o.UserInfo, UserInfo).Create();
            Command.ApprenticeshipId = ApprenticeshipId;
            Command.AccountId = 222;

            Handler = new UndoApprenticeshipUpdatesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                AuthenticationService.Object,
                Mock.Of<ILogger<UndoApprenticeshipUpdatesCommandHandler>>());

            var _ = SeedData().Result;
        }

        public async Task Handle()
        {
            try
            {
                await Handler.Handle(Command, CancellationToken);
            }
            catch(Exception exception)
            {
                Exception = exception;
            }
        }

        private async Task<UndoApprenticeshipUpdatesCommandHandlerTestsFixture> SeedData()
        {
            Db.Apprenticeships.Add(ApprenticeshipDetails);

            await Db.SaveChangesAsync();
            return this;
        }

        public ApprenticeshipUpdate GetApprenticeshipUpdate()
        {
            var apprenticeshipUpdate = new ApprenticeshipUpdate()
           .Set(c => c.Id, 555)
           .Set(c => c.ApprenticeshipId, ApprenticeshipId);

            return apprenticeshipUpdate;
        }

        public async Task<UndoApprenticeshipUpdatesCommandHandlerTestsFixture> AddANewApprenticeshipUpdate(ApprenticeshipUpdate update)
        {
            var apprenticeship = Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
          
            apprenticeship.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            apprenticeship.ApprenticeshipUpdate.Add(update);

            await Db.SaveChangesAsync();
            return this;
        }

        public void VerifyException<T>()
        {
            Assert.That(Exception, Is.Not.Null);
            Assert.That(Exception, Is.InstanceOf<T>());
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
