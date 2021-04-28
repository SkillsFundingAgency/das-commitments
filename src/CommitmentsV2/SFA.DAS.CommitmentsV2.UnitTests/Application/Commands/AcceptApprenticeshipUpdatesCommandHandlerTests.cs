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
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AcceptApprenticeshipUpdatesCommandHandlerTests 
    {
        AcceptApprenticeshipUpdatesCommandHandlerTestsFixture fixture;
        //[SetUp]
        //public async Task Setup()
        //{

        //}

        [Test]
        public async Task Handle_WhenCommandIsHandled_ExceptionIsThrown_IfNoApprenticeshipUpdates()
        {
           await fixture.Handle();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_FirstNameIsUpdate()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await fixture.SeedData();
            fixture.ApprenticeshipUpdate.FirstName = "XXX";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();
        }

        //[Test]
        //public Task Handle_WhenCommandIsHandled_ThenShouldReturnAddDraftApprenticeshipResult()
        //{
        //    //return TestAsync(
        //    //    f => f.AddDraftApprenticeship(),
        //    //    (f, r) => r.Should().NotBeNull().And.Subject.Should().Match<AddDraftApprenticeshipResult>(r2 => r2.Id == f.DraftApprenticeship.Id));
        //}
    }

    public class AcceptApprenticeshipUpdatesCommandHandlerTestsFixture
    {
        public long ApprenticeshipId = 12;
        public Fixture fixture { get; set; }
        public AcceptApprenticeshipUpdatesCommand Command { get; set; }
        public Apprenticeship ApprenticeshipDetails { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<AcceptApprenticeshipUpdatesCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public Mock<IAuthenticationService> AuthenticationService;
        public Mock<IOverlapCheckService> OverlapCheckService;
        public Party Party;
        public bool HasOverlapErrors;
        public ApprenticeshipUpdate ApprenticeshipUpdate;
        public IUnitOfWorkContext UnitOfWorkContext { get; set; }

        public AcceptApprenticeshipUpdatesCommandHandlerTestsFixture()
        {
            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Party = Party.Employer;
            HasOverlapErrors = false;
            UnitOfWorkContext = new CommitmentsV2.Models.UnitOfWorkContext2();
            // ApprenticeshipDetails = fixture.Create<Apprenticeship>();

            var Cohort = new CommitmentsV2.Models.Cohort()
              .Set(c => c.Id, 111)
              .Set(c => c.EmployerAccountId, 222)
              .Set(c => c.ProviderId, 333)
              .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

            ApprenticeshipUpdate = new ApprenticeshipUpdate()
            .Set(c => c.Id, 555)
            .Set(c => c.ApprenticeshipId, ApprenticeshipId)
            .Set(c => c.FirstName, "XXXX");

            //List<CommitmentsV2.Models.ApprenticeshipUpdate> list = new List<ApprenticeshipUpdate>();
            //list.Add(apprenticeshipUpdate);

            ApprenticeshipDetails = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.Cohort, Cohort)
             .With(s => s.PaymentStatus, PaymentStatus.Completed)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Create();

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);
            
            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), ApprenticeshipId, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new OverlapCheckResult(HasOverlapErrors, HasOverlapErrors)));

            UserInfo = fixture.Create<UserInfo>();
            Command = fixture.Build<AcceptApprenticeshipUpdatesCommand>().With(o => o.UserInfo, UserInfo).Create();
            Command.ApprenticeshipId = ApprenticeshipId;
            Command.AccountId = 222;

            

            Handler = new AcceptApprenticeshipUpdatesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                AuthenticationService.Object,
                OverlapCheckService.Object,
                Mock.Of<ILogger<AcceptApprenticeshipUpdatesCommandHandler>>());

            var xx = SeedData().Result;
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken);
        }

        public async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> SeedData()
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

        public async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> AddANewApprenticeshipUpdate(ApprenticeshipUpdate update)
        {
            var apprenticeship = Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
            apprenticeship.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            apprenticeship.ApprenticeshipUpdate.Add(update);

            await Db.SaveChangesAsync();
            return this;
        }
    }
}