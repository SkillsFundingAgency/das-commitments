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
        private AcceptApprenticeshipUpdatesCommandHandlerTestsFixture _fixture;

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

            Assert.That(_fixture.ApprenticeshipFromDb.FirstName, Is.EqualTo("XXX"));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_LastNameIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.LastName = "XXX";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.LastName, Is.EqualTo("XXX"));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DoBIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DateOfBirth = new DateTime(2000, 1, 1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.DateOfBirth, Is.EqualTo(_fixture.ApprenticeshipUpdate.DateOfBirth));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EmailIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Email = "XXX@XX.com";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.Email, Is.EqualTo("XXX@XX.com"));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_StartDateIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.StartDate = new DateTime(2000, 1, 1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ApprenticeshipFromDb.StartDate, Is.EqualTo(_fixture.ApprenticeshipUpdate.StartDate));
                Assert.That(_fixture.PriceHistoryFromDb.FromDate, Is.EqualTo(_fixture.ApprenticeshipUpdate.StartDate));
            });
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_EndDateIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.EndDate = new DateTime(2000, 1, 1);
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.EndDate, Is.EqualTo(_fixture.ApprenticeshipUpdate.EndDate));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DeliveryModelPortableFlexiJobIsCorrectlyUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DeliveryModel = DeliveryModel.PortableFlexiJob;
            _fixture.ApprenticeshipUpdate.EmploymentEndDate = DateTime.UtcNow;
            ;
            _fixture.ApprenticeshipUpdate.EmploymentPrice = 10001;
            ;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ApprenticeshipFromDb.DeliveryModel, Is.EqualTo(_fixture.ApprenticeshipUpdate.DeliveryModel));
                Assert.That(_fixture.ApprenticeshipFromDb.FlexibleEmployment?.EmploymentEndDate, Is.EqualTo(_fixture.ApprenticeshipUpdate.EmploymentEndDate));
                Assert.That(_fixture.ApprenticeshipFromDb.FlexibleEmployment?.EmploymentPrice, Is.EqualTo(_fixture.ApprenticeshipUpdate.EmploymentPrice));
            });
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_DeliveryModelRegularIsCorrectlyUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.DeliveryModel = DeliveryModel.Regular;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ApprenticeshipFromDb.DeliveryModel, Is.EqualTo(_fixture.ApprenticeshipUpdate.DeliveryModel));
                Assert.That(_fixture.ApprenticeshipUpdate.EmploymentEndDate, Is.Null);
                Assert.That(_fixture.ApprenticeshipUpdate.EmploymentPrice, Is.Null);
            });
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CourseCodeIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCode = "195";
            _fixture.ApprenticeshipUpdate.TrainingName = "DummyTraining";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ApprenticeshipFromDb.CourseCode, Is.EqualTo("195"));
                Assert.That(_fixture.ApprenticeshipFromDb.CourseName, Is.EqualTo("DummyTraining"));
            });
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_CostIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(_fixture.ApprenticeshipFromDb.Cost, Is.EqualTo(195));
                Assert.That(_fixture.PriceHistoryFromDb.Cost, Is.EqualTo(195));
            });
        }

        [TestCase("Option")]
        [TestCase("")]
        public async Task Handle_WhenOptionIsNotNull_OptionIsUpdated(string option)
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = option;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.TrainingCourseOption, Is.EqualTo(option));
        }

        [Test]
        public async Task Handle_WhenCourseHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCode = "123";
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.TrainingCourseOption, Is.Null);
        }

        [Test]
        public async Task Handle_WhenVersionHasChanged_And_OptionIsNull_Then_OptionIsUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseVersion = "2.0";
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.TrainingCourseOption, Is.Null);
        }

        [Test]
        public async Task Handle_WhenCourseAndVersionHasNotChanged_And_OptionIsNull_Then_OptionIsNotUpdated()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.TrainingCourseOption = null;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.TrainingCourseOption, Is.EqualTo(_fixture.ApprenticeshipDetails.TrainingCourseOption));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_PendingOriginatorIsNULL()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipFromDb.PendingUpdateOriginator, Is.EqualTo(null));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ApprenticeshipUpdateStatus_IsApproved()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 195;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That(_fixture.ApprenticeshipUpdate.Status, Is.EqualTo(ApprenticeshipUpdateStatus.Approved));
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

            Assert.Multiple(() =>
            {
                Assert.That(list, Has.Count.EqualTo(1));
                Assert.That(list[0].ApprenticeshipId, Is.EqualTo(apprenticeship.Id));
                Assert.That(list[0].ApprovedOn, Is.EqualTo(_fixture.ProxyCurrentDateTime));
                Assert.That(list[0].StartDate, Is.EqualTo(apprenticeship.StartDate));
                Assert.That(list[0].EndDate, Is.EqualTo(apprenticeship.EndDate));
                Assert.That(list[0].TrainingType, Is.EqualTo(apprenticeship.ProgrammeType as ProgrammeType?));
                Assert.That(list[0].DeliveryModel, Is.EqualTo(apprenticeship.DeliveryModel));
                Assert.That(list[0].EmploymentEndDate, Is.EqualTo(apprenticeship.FlexibleEmployment?.EmploymentEndDate));
                Assert.That(list[0].EmploymentPrice, Is.EqualTo(apprenticeship.FlexibleEmployment?.EmploymentPrice));
                Assert.That(list[0].DeliveryModel, Is.EqualTo(apprenticeship.DeliveryModel));
                Assert.That(list[0].EmploymentEndDate, Is.EqualTo(apprenticeship.FlexibleEmployment?.EmploymentEndDate));
                Assert.That(list[0].EmploymentPrice, Is.EqualTo(apprenticeship.FlexibleEmployment?.EmploymentPrice));
                Assert.That(list[0].TrainingCode, Is.EqualTo(apprenticeship.CourseCode));
                Assert.That(list[0].Uln, Is.EqualTo(apprenticeship.Uln));
                Assert.That(list[0].PriceEpisodes, Has.Length.EqualTo(1));
                Assert.That(list[0].PriceEpisodes[0].FromDate, Is.EqualTo(priceEpisode[0].FromDate));
                Assert.That(list[0].PriceEpisodes[0].ToDate, Is.EqualTo(priceEpisode[0].ToDate));
                Assert.That(list[0].PriceEpisodes[0].Cost, Is.EqualTo(priceEpisode[0].Cost));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(list, Has.Count.EqualTo(1));
                Assert.That(list[0].ApprenticeshipId, Is.EqualTo(apprenticeship.Id));
                Assert.That(list[0].ApprovedOn, Is.EqualTo(_fixture.ProxyCurrentDateTime));
            });
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_AndEmailIsNotUpdated_ApprenticeshipUpdatedEmailAddressEvent_IsNotEmitted()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            _fixture.ApprenticeshipUpdate.Cost = 192;
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            var list = _fixture.UnitOfWorkContext.GetEvents().OfType<ApprenticeshipUpdatedEmailAddressEvent>().ToList();

            Assert.That(list, Is.Empty);
        }

        [Test]
        public async Task ThenEmailAddressCannotBeChangedWhenEmailAddressIsConfirmed()
        {
            _fixture = new AcceptApprenticeshipUpdatesCommandHandlerTestsFixture();
            await _fixture.SetEmailAddressConfirmedByApprentice();
            _fixture.ApprenticeshipUpdate.Email = "test@test.com";
            await _fixture.AddANewApprenticeshipUpdate(_fixture.ApprenticeshipUpdate);

            await _fixture.Handle();

            Assert.That((_fixture.Exception as DomainException)?.DomainErrors.First().ErrorMessage, Is.EqualTo("Unable to approve these changes, as the apprentice has confirmed their email address"));
        }
    }

    public class AcceptApprenticeshipUpdatesCommandHandlerTestsFixture : IDisposable
    {
        public long ApprenticeshipId = 12;
        public Fixture Fixture { get; set; }
        public AcceptApprenticeshipUpdatesCommand Command { get; set; }
        public Apprenticeship ApprenticeshipDetails { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<AcceptApprenticeshipUpdatesCommand> Handler { get; set; }
        public UserInfo UserInfo { get; }
        public Mock<IAuthenticationService> AuthenticationService;
        public Mock<ICurrentDateTime> CurrentDateTimeService;
        public Mock<IOverlapCheckService> OverlapCheckService;
        public Party Party;
        public bool HasOverlapErrors;
        public ApprenticeshipUpdate ApprenticeshipUpdate;
        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public Apprenticeship ApprenticeshipFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);
        public PriceHistory PriceHistoryFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId).PriceHistory.First();
        public Exception Exception { get; set; }

        public DateTime ProxyCurrentDateTime = new(2020, 1, 1);

        public AcceptApprenticeshipUpdatesCommandHandlerTestsFixture()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            Party = Party.Employer;
            HasOverlapErrors = false;
            UnitOfWorkContext = new UnitOfWorkContext();

            var cohort = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

            ApprenticeshipUpdate = new ApprenticeshipUpdate()
                .Set(c => c.ApprenticeshipId, ApprenticeshipId)
                .Set(c => c.Status, ApprenticeshipUpdateStatus.Pending);

            var priceHistory = new List<PriceHistory>()
            {
                new()
                {
                    FromDate = DateTime.Now,
                    ToDate = null,
                    Cost = 10000,
                }
            };

            ApprenticeshipDetails = Fixture.Build<Apprenticeship>()
                .With(s => s.Id, ApprenticeshipId)
                .With(s => s.Cohort, cohort)
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), ApprenticeshipId, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(new OverlapCheckResult(HasOverlapErrors, HasOverlapErrors)));

            CurrentDateTimeService = new Mock<ICurrentDateTime>();
            CurrentDateTimeService.Setup(x => x.UtcNow).Returns(ProxyCurrentDateTime);

            UserInfo = Fixture.Create<UserInfo>();
            Command = Fixture.Build<AcceptApprenticeshipUpdatesCommand>().With(o => o.UserInfo, UserInfo).Create();
            Command.ApprenticeshipId = ApprenticeshipId;
            Command.AccountId = 222;

            Handler = new AcceptApprenticeshipUpdatesCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                AuthenticationService.Object,
                OverlapCheckService.Object,
                CurrentDateTimeService.Object,
                Mock.Of<ILogger<AcceptApprenticeshipUpdatesCommandHandler>>());

            var _ = SeedData().Result;
        }

        public async Task Handle()
        {
            try
            {
                await Handler.Handle(Command, CancellationToken);
            }
            catch (Exception exception)
            {
                Exception = exception;
            }
        }

        public async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> SetEmailAddressConfirmedByApprentice()
        {
            var first = Db.Apprenticeships.First();
            first.EmailAddressConfirmed = true;
            await Db.SaveChangesAsync(CancellationToken);
            return this;
        }

        private async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> SeedData()
        {
            Db.Apprenticeships.Add(ApprenticeshipDetails);

            await Db.SaveChangesAsync(CancellationToken);
            return this;
        }
        
        public async Task<AcceptApprenticeshipUpdatesCommandHandlerTestsFixture> AddANewApprenticeshipUpdate(ApprenticeshipUpdate update)
        {
            var apprenticeship = Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);

            apprenticeship.ApprenticeshipUpdate = new List<ApprenticeshipUpdate>();
            apprenticeship.ApprenticeshipUpdate.Add(update);

            await Db.SaveChangesAsync(CancellationToken);
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