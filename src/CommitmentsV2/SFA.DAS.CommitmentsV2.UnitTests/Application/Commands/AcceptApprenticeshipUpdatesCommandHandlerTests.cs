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
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
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
    public class AcceptApprenticeshipUpdatesCommandHandlerTests 
    {
        AcceptApprenticeshipUpdatesCommandHandlerTestsFixture fixture;

        [Test]
        public async Task Handle_WhenCommandIsHandled_FirstNameIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.FirstName = "XXX";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual("XXX", fixture.ApprenticeshipFromDb.FirstName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_LastNameIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.LastName = "XXX";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual("XXX", fixture.ApprenticeshipFromDb.LastName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DoBIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.DateOfBirth = new DateTime(2000,1,1);
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DateOfBirth, fixture.ApprenticeshipFromDb.DateOfBirth);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EmailIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Email = "XXX@XX.com";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual("XXX@XX.com", fixture.ApprenticeshipFromDb.Email);
        }



        [Test]
        public async Task Handle_WhenCommandIsHandled_StartDateIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.StartDate = new DateTime(2000, 1, 1);
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.StartDate, fixture.ApprenticeshipFromDb.StartDate);
            Assert.AreEqual(fixture.ApprenticeshipUpdate.StartDate, fixture.PriceHistoryFromDb.FromDate);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EndDateIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.EndDate = new DateTime(2000, 1, 1);
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.EndDate, fixture.ApprenticeshipFromDb.EndDate);
        }

        [TestCase(DeliveryModel.Normal)]
        [TestCase(DeliveryModel.Flexible)]
        public async Task Handle_WhenCommandIsHandled_DeliveryModelIsUpdated(DeliveryModel dm)
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.DeliveryModel = dm;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(fixture.ApprenticeshipUpdate.DeliveryModel, fixture.ApprenticeshipFromDb.DeliveryModel);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CourseCodeIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.TrainingCode = "195";
            fixture.ApprenticeshipUpdate.TrainingName = "DummyTraining";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual("195", fixture.ApprenticeshipFromDb.CourseCode);
            Assert.AreEqual("DummyTraining", fixture.ApprenticeshipFromDb.CourseName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CostIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(195, fixture.ApprenticeshipFromDb.Cost);
            Assert.AreEqual(195, fixture.PriceHistoryFromDb.Cost);
        }

        [TestCase("Option")]
        [TestCase("")]
        public async Task Handle_WhenOptionIsNotNull_OptionIsUpdated(string option)
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.TrainingCourseOption = option;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(option, fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCourseHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.TrainingCode = "123";
            fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.IsNull(fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenVersionHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.TrainingCourseVersion = "2.0";
            fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.IsNull(fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCourseAndVersionHasNotChanged_And_OptionIsNull_Then_OptionIsNotUpdated()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(fixture.ApprenticeshipDetails.TrainingCourseOption, fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_PendingOriginatorIsNULL()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(null, fixture.ApprenticeshipFromDb.PendingUpdateOriginator);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdateStatus_IsApproved()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual(ApprenticeshipUpdateStatus.Approved, fixture.ApprenticeshipUpdate.Status);
        }

        [Test]
        public async Task Handle_WhenNoApprenticeshipUpdate_AndCommandIsHandled_ExceptionIsThrown()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyException<InvalidOperationException>();
        }

        [Test]
        public async Task Handle_WhenHasUlnOverlap_AndCommandIsHandled_ExceptionIsThrown()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.HasOverlapErrors = true;
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdatedApprovedEvent_IsEmitted()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 195;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            var list = fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedApprovedEvent>().ToList();

            var apprenticeship = fixture.ApprenticeshipFromDb;
            var priceEpisode = apprenticeship.PriceHistory.Select(x => new PriceEpisode
            {
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                Cost = x.Cost
            }).ToArray();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(apprenticeship.Id, list[0].ApprenticeshipId);
            Assert.AreEqual(fixture.proxyCurrentDateTime, list[0].ApprovedOn);
            Assert.AreEqual(apprenticeship.StartDate, list[0].StartDate);
            Assert.AreEqual(apprenticeship.EndDate, list[0].EndDate);
            Assert.AreEqual(apprenticeship.ProgrammeType as SFA.DAS.CommitmentsV2.Types.ProgrammeType?, list[0].TrainingType);
            Assert.AreEqual(apprenticeship.CourseCode, list[0].TrainingCode);
            Assert.AreEqual(apprenticeship.Uln, list[0].Uln);
            Assert.AreEqual(1, list[0].PriceEpisodes.Count());
            Assert.AreEqual(priceEpisode[0].FromDate, list[0].PriceEpisodes[0].FromDate);
            Assert.AreEqual(priceEpisode[0].ToDate, list[0].PriceEpisodes[0].ToDate);
            Assert.AreEqual(priceEpisode[0].Cost, list[0].PriceEpisodes[0].Cost);
        }


        [Test]
        public async Task Handle_WhenCommandIsHandled_AndEmailIsUpdated_ApprenticeshipUpdatedEmailAddressEvent_IsEmitted()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Email = "new@email.com";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            var list = fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedEmailAddressEvent>().ToList();

            var apprenticeship = fixture.ApprenticeshipFromDb;

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(apprenticeship.Id, list[0].ApprenticeshipId);
            Assert.AreEqual(fixture.proxyCurrentDateTime, list[0].ApprovedOn);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_AndEmailIsNotUpdated_ApprenticeshipUpdatedEmailAddressEvent_IsNotEmitted()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            fixture.ApprenticeshipUpdate.Cost = 192;
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            var list = fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedEmailAddressEvent>().ToList();

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task ThenEmailAddressCannotBeChangedWhenEmailAddressIsConfirmed()
        {
            fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await fixture.SetEmailAddressConfirmedByApprentice();
            fixture.ApprenticeshipUpdate.Email = "test@test.com";
            await fixture.AddANewApprenticeshipUpdate(fixture.ApprenticeshipUpdate);

            await fixture.Handle();

            Assert.AreEqual((fixture.Exception as DomainException).DomainErrors.First().ErrorMessage, "Unable to approve these changes, as the apprentice has confirmed their email address");
        }
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
        public Mock<ICurrentDateTime> currentDateTimeService;
        public Mock<IOverlapCheckService> OverlapCheckService;
        public Party Party;
        public bool HasOverlapErrors;
        public ApprenticeshipUpdate ApprenticeshipUpdate;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public Apprenticeship ApprenticeshipFromDb => 
            Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
        public PriceHistory PriceHistoryFromDb =>
          Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).PriceHistory.First();
        public ApprenticeshipUpdate ApprenticeshipUpdateFromDb =>
          Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).ApprenticeshipUpdate.First();

        public Exception Exception { get; set; }

        public DateTime proxyCurrentDateTime = new DateTime(2020, 1, 1);

        public AcceptApprenticeshipUpdatesCommandHandlerTestsFixture()
        {
            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Party = Party.Employer;
            HasOverlapErrors = false;
            UnitOfWorkContext = new UnitOfWorkContext();

            var Cohort = new CommitmentsV2.Models.Cohort()
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

            ApprenticeshipDetails = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
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
             .Without(s => s.EmailAddressConfirmed)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);
            
            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), ApprenticeshipId, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new OverlapCheckResult(HasOverlapErrors, HasOverlapErrors)));

            currentDateTimeService = new Mock<ICurrentDateTime>();
            currentDateTimeService.Setup(x => x.UtcNow).Returns(proxyCurrentDateTime);

            UserInfo = fixture.Create<UserInfo>();
            Command = fixture.Build<AcceptApprenticeshipUpdatesCommand>().With(o => o.UserInfo, UserInfo).Create();
            Command.ApprenticeshipId = ApprenticeshipId;
            Command.AccountId = 222;

            Handler = new AcceptApprenticeshipUpdatesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                AuthenticationService.Object,
                OverlapCheckService.Object,
                currentDateTimeService.Object,
                Mock.Of<ILogger<AcceptApprenticeshipUpdatesCommandHandler>>());

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

        public async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> SetEmailAddressConfirmedByApprentice()
        {
            var first = Db.Apprenticeships.First();
            first.EmailAddressConfirmed = true;
            await Db.SaveChangesAsync();
            return this;
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

        public void VerifyException<T>()
        {
            Assert.IsNotNull(Exception);
            Assert.IsInstanceOf<T>(Exception);
        }
    }
}