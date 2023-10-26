using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        AcceptApprenticeshipUpdatesCommandHandlerTestsFixture _fixture;

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_FirstNameIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.FirstName = "XXX";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual("XXX", _fixture.ApprenticeshipFromDb.FirstName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_LastNameIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.LastName = "XXX";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual("XXX", _fixture.ApprenticeshipFromDb.LastName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DoBIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DateOfBirth = new DateTime(2000,1,1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipUpdate.DateOfBirth, _fixture.ApprenticeshipFromDb.DateOfBirth);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EmailIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Email = "XXX@XX.com";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual("XXX@XX.com", _fixture.ApprenticeshipFromDb.Email);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_StartDateIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.StartDate = new DateTime(2000, 1, 1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipUpdate.StartDate, _fixture.ApprenticeshipFromDb.StartDate);
            Assert.AreEqual(_fixture.ApprenticeshipUpdate.StartDate, _fixture.PriceHistoryFromDb.FromDate);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EndDateIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.EndDate = new DateTime(2000, 1, 1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipUpdate.EndDate, _fixture.ApprenticeshipFromDb.EndDate);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DeliveryModelPortableFlexiJobIsCorrectlyUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DeliveryModel = DeliveryModel.PortableFlexiJob;
            _fixture.ApprenticeshipUpdate.EmploymentEndDate = DateTime.UtcNow;;
            _fixture.ApprenticeshipUpdate.EmploymentPrice = 10001;;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipUpdate.DeliveryModel, _fixture.ApprenticeshipFromDb.DeliveryModel);
            Assert.AreEqual(_fixture.ApprenticeshipUpdate.EmploymentEndDate, _fixture.ApprenticeshipFromDb.FlexibleEmployment?.EmploymentEndDate);
            Assert.AreEqual(_fixture.ApprenticeshipUpdate.EmploymentPrice, _fixture.ApprenticeshipFromDb.FlexibleEmployment?.EmploymentPrice);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DeliveryModelRegularIsCorrectlyUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DeliveryModel = DeliveryModel.Regular;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipUpdate.DeliveryModel, _fixture.ApprenticeshipFromDb.DeliveryModel);
            Assert.IsNull(_fixture.ApprenticeshipUpdate.EmploymentEndDate);
            Assert.IsNull(_fixture.ApprenticeshipUpdate.EmploymentPrice);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CourseCodeIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCode = "195";
            _fixture.ApprenticeshipUpdate.TrainingName = "DummyTraining";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual("195", _fixture.ApprenticeshipFromDb.CourseCode);
            Assert.AreEqual("DummyTraining", _fixture.ApprenticeshipFromDb.CourseName);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CostIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(195, _fixture.ApprenticeshipFromDb.Cost);
            Assert.AreEqual(195, _fixture.PriceHistoryFromDb.Cost);
        }

        [TestCase("Option")]
        [TestCase("")]
        public async Task Handle_WhenOptionIsNotNull_OptionIsUpdated(string option)
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = option;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(option, _fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCourseHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCode = "123";
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.IsNull(_fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenVersionHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseVersion = "2.0";
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.IsNull(_fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCourseAndVersionHasNotChanged_And_OptionIsNull_Then_OptionIsNotUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(_fixture.ApprenticeshipDetails.TrainingCourseOption, _fixture.ApprenticeshipFromDb.TrainingCourseOption);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_PendingOriginatorIsNULL()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(null, _fixture.ApprenticeshipFromDb.PendingUpdateOriginator);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdateStatus_IsApproved()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual(ApprenticeshipUpdateStatus.Approved, _fixture.ApprenticeshipUpdate.Status);
        }

        [Test]
        public async Task Handle_WhenNoApprenticeshipUpdate_AndCommandIsHandled_ExceptionIsThrown()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await _fixture.Handle();

            _fixture.VerifyException<InvalidOperationException>();
        }

        [Test]
        public async Task Handle_WhenHasUlnOverlap_AndCommandIsHandled_ExceptionIsThrown()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.HasOverlapErrors = true;
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            _fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdatedApprovedEvent_IsEmitted()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            _fixture.ApprenticeshipDetails.DeliveryModel = DeliveryModel.PortableFlexiJob;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            var list = _fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedApprovedEvent>().ToList();

            var apprenticeship = _fixture.ApprenticeshipFromDb;
            var priceEpisode = apprenticeship.PriceHistory.Select(x => new PriceEpisode
            {
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                Cost = x.Cost
            }).ToArray();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(apprenticeship.Id, list[0].ApprenticeshipId);
            Assert.AreEqual(_fixture.proxyCurrentDateTime, list[0].ApprovedOn);
            Assert.AreEqual(apprenticeship.StartDate, list[0].StartDate);
            Assert.AreEqual(apprenticeship.EndDate, list[0].EndDate);
            Assert.AreEqual(apprenticeship.ProgrammeType as ProgrammeType?, list[0].TrainingType);
            Assert.AreEqual(apprenticeship.DeliveryModel, list[0].DeliveryModel);
            Assert.AreEqual(apprenticeship.FlexibleEmployment?.EmploymentEndDate, list[0].EmploymentEndDate);
            Assert.AreEqual(apprenticeship.FlexibleEmployment?.EmploymentPrice, list[0].EmploymentPrice);
            Assert.AreEqual(apprenticeship.DeliveryModel, list[0].DeliveryModel);
            Assert.AreEqual(apprenticeship.FlexibleEmployment?.EmploymentEndDate, list[0].EmploymentEndDate);
            Assert.AreEqual(apprenticeship.FlexibleEmployment?.EmploymentPrice, list[0].EmploymentPrice);
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
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Email = "new@email.com";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            var list = _fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedEmailAddressEvent>().ToList();

            var apprenticeship = _fixture.ApprenticeshipFromDb;

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(apprenticeship.Id, list[0].ApprenticeshipId);
            Assert.AreEqual(_fixture.proxyCurrentDateTime, list[0].ApprovedOn);
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_AndEmailIsNotUpdated_ApprenticeshipUpdatedEmailAddressEvent_IsNotEmitted()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 192;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            var list = _fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedEmailAddressEvent>().ToList();

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task ThenEmailAddressCannotBeChangedWhenEmailAddressIsConfirmed()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await _fixture.SetEmailAddressConfirmedByApprentice();
            _fixture.ApprenticeshipUpdate.Email = "test@test.com";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.AreEqual((_fixture.Exception as DomainException).DomainErrors.First().ErrorMessage, "Unable to approve these changes, as the apprentice has confirmed their email address");
        }
    }

    public class AcceptApprenticeshipUpdatesCommandHandlerTestsFixture : IDisposable
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

        public Apprenticeship ApprenticeshipFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
        public PriceHistory PriceHistoryFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).PriceHistory.First();
        public Exception Exception { get; set; }

        public DateTime proxyCurrentDateTime = new DateTime(2020, 1, 1);

        public AcceptApprenticeshipUpdatesCommandHandlerTestsFixture()
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

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}