using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class EditApprenticeshipCommandHandlerTests
    {
        EditApprenticeshipCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new EditApprenticeshipCommandHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task OnlyImmediateUpdate_WhenOnlyEmployerReferenceIsChanged()
        {
            _fixture.SetParty(Party.Employer);
            _fixture.Command.EditApprenticeshipRequest.EmployerReference = "NewEmployerRef";

            await _fixture.Handle();
            _fixture.VerifyOnlyEmployerImmediateUpdate();
        }

        [Test]
        public async Task OnlyImmediateUpdate_WhenOnlyProviderReferenceIsChanged()
        {
            _fixture.SetParty(Party.Provider);
            _fixture.Command.EditApprenticeshipRequest.ProviderReference = "NewProviderRef";

            await _fixture.Handle();
            _fixture.VerifyOnlyProviderImmediateUpdate();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenFirstNameIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.FirstName = "NewFirstName";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("NewFirstName", app => app.ApprenticeshipUpdate.First().FirstName);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenLastNameIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.LastName = "NewLastName";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("NewLastName", app => app.ApprenticeshipUpdate.First().LastName);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmailAddressIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.Email = "New@mail.com";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("New@mail.com", app => app.ApprenticeshipUpdate.First().Email);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmailAddressCannotBeChangedWhenEmailAddressConfirmedByApprentice(Party party)
        {
            _fixture.SetParty(party).SetEmailAddressConfirmedByApprentice();
            _fixture.Command.EditApprenticeshipRequest.Email = "New@mail.com";

            try
            {
                await _fixture.Handle();
                Assert.Fail();
            }
            catch (DomainException ex)
            {
                ex.DomainErrors.First().ErrorMessage.Should()
                    .Be(
                        "Unable to make these changes, as the apprentice has confirmed their email address");
            }
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenDobIsChanged(Party party)

        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.DateOfBirth = DateTime.UtcNow;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.DateOfBirth.Value,
                app => app.ApprenticeshipUpdate.First().DateOfBirth.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenDeliveryModelIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.DeliveryModel = DeliveryModel.PortableFlexiJob;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.DeliveryModel, app => app.ApprenticeshipUpdate.First().DeliveryModel);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task And_DeliveryModelIsSetToRegular_ThenDeliveryModelIsChanged_And_EmploymentFieldsSetToNull(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.DeliveryModel = DeliveryModel.Regular;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.DeliveryModel, app => app.ApprenticeshipUpdate.First().DeliveryModel);
            _fixture.VerifyEmploymentFieldsAreNull();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmploymentEndDateIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.DeliveryModel = DeliveryModel.PortableFlexiJob;
            _fixture.Command.EditApprenticeshipRequest.EmploymentEndDate = DateTime.UtcNow;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.EmploymentEndDate.Value,
                app => app.ApprenticeshipUpdate.First().EmploymentEndDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmploymentPriceIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.DeliveryModel = DeliveryModel.PortableFlexiJob;
            _fixture.Command.EditApprenticeshipRequest.EmploymentPrice = 100;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated((long)_fixture.Command.EditApprenticeshipRequest.EmploymentPrice,
                app => (long)app.ApprenticeshipUpdate.First().EmploymentPrice);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenStartDateIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.StartDate = DateTime.UtcNow;
            
            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.StartDate.Value,
                app => app.ApprenticeshipUpdate.First().StartDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEndDateIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.EndDate = DateTime.UtcNow;
         
            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.EndDate.Value,
                app => app.ApprenticeshipUpdate.First().EndDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task AndEmailAddressConfirmedThenEndDateIsChanged(Party party)
        {
            _fixture.SetParty(party).SetEmailAddressConfirmedByApprentice();
            _fixture.Command.EditApprenticeshipRequest.EndDate = DateTime.UtcNow;

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.EndDate.Value,
                app => app.ApprenticeshipUpdate.First().EndDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenCourseCodeIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("NewCourse",
                app => app.ApprenticeshipUpdate.First().TrainingCode);
            _fixture.VerifyApprenticeshipUpdateCreated("CourseName",
              app => app.ApprenticeshipUpdate.First().TrainingName);
            _fixture.VerifyApprenticeshipUpdateCreated((int)ProgrammeType.Standard,
            app => (int) app.ApprenticeshipUpdate.First().TrainingType);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenMultipleAreChangedIsChanged(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";
            _fixture.Command.EditApprenticeshipRequest.EndDate = DateTime.UtcNow;
            _fixture.Command.EditApprenticeshipRequest.LastName = "NewLastName";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("NewCourse",
                app => app.ApprenticeshipUpdate.First().TrainingCode);
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Command.EditApprenticeshipRequest.EndDate.Value,
                 app => app.ApprenticeshipUpdate.First().EndDate.Value);
            _fixture.VerifyApprenticeshipUpdateCreated("NewLastName", app => app.ApprenticeshipUpdate.First().LastName);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task NotChangedFieldsAreNull_InApprenticehipUpdateTable(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().FirstName);
            _fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().LastName);
            _fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().Cost?.ToString());
            _fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().DateOfBirth);
            _fixture.VerifyApprenticeshipUpdateCreated(null,
               app => app.ApprenticeshipUpdate.First().StartDate);
            _fixture.VerifyApprenticeshipUpdateCreated(null,
               app => app.ApprenticeshipUpdate.First().EndDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task Then_OriginatorIsSetTo_PartyMakingTheChange(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated( party == Party.Employer ? (int)Originator.Employer : (int)Originator.Provider,
                app => (int)app.ApprenticeshipUpdate.First().Originator);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateOriginIsSetTo_ChangeOfCircumstances(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated((int)Types.ApprenticeshipUpdateOrigin.ChangeOfCircumstances,
                app => (int)app.ApprenticeshipUpdate.First().UpdateOrigin);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task EffectiveFromDate_Is_Set_To_Apprenticeship_StartDate(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(_fixture.Db.Apprenticeships.Where(x => x.Id == _fixture.ApprenticeshipId).First().StartDate,
                app => app.ApprenticeshipUpdate.First().EffectiveFromDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task EffectiveToDate_Is_Set_To_Null(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "123";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().EffectiveToDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task When_NewStandardIsSelected_Then_SetStandardUIdAndVersion(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "123";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated("123",
                app => app.ApprenticeshipUpdate.First().TrainingCode);
            _fixture.VerifyApprenticeshipUpdateCreated("ST0123_1.0",
                app => app.ApprenticeshipUpdate.First().StandardUId);
            _fixture.VerifyApprenticeshipUpdateCreated("1.0",
                app => app.ApprenticeshipUpdate.First().TrainingCourseVersion);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task CreatedOn_Is_Set_To_DateTimeNow(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            var dateTimeNow = DateTime.Now;
            _fixture.currentDateTime.Setup(x => x.UtcNow).Returns(dateTimeNow);

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreated(dateTimeNow,
                app => app.ApprenticeshipUpdate.First().CreatedOn);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task Published_ApprenticeshipUpdateCreatedEvent(Party party)
        {
            _fixture.SetParty(party);
            _fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipUpdateCreatedEvent();
        }
    }

    public class EditApprenticeshipCommandHandlerTestsFixture : IDisposable
    {
        public Mock<IEditApprenticeshipValidationService> editApprenticeshipValidationService { get; set; }
        public Mock<ICurrentDateTime> currentDateTime { get; set; }
        public EditApprenticeshipCommand Command { get; set; }
        public IRequestHandler<EditApprenticeshipCommand, EditApprenticeshipResponse> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IMediator> mediator { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public Party Party { get; set; }
        public long ApprenticeshipId { get; set; }

        public EditApprenticeshipCommandHandlerTestsFixture()
        {
            Party = Party.Employer;
            UnitOfWorkContext = new UnitOfWorkContext();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options);
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var cohort = new Cohort()
               .Set(c => c.Id, 111)
               .Set(c => c.EmployerAccountId, 222)
               .Set(c => c.ProviderId, 333)
               .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

            var apprenticeship = fixture.Build<Apprenticeship>()
             .With(s => s.Cohort, cohort)
             .With(s => s.PaymentStatus, PaymentStatus.Active)
             .With(s => s.EndDate, DateTime.UtcNow.AddYears(1))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Without(s => s.CompletionDate)
             .Without(s => s.EmailAddressConfirmed)
             .Without(s => s.ApprenticeshipConfirmationStatus)
             .Create();

            Db.Apprenticeships.Add(apprenticeship);

            Db.SaveChanges();

            ApprenticeshipId = apprenticeship.Id;

            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(x => x.GetUserParty()).Returns(() => Party);

            var lazyProviderDbContext = new Lazy<ProviderCommitmentsDbContext>(() => Db);

            var newEndDate = apprenticeship.EndDate.Value.AddDays(1);

            Command = new EditApprenticeshipCommand
            {
                EditApprenticeshipRequest = new Api.Types.Requests.EditApprenticeshipApiRequest
                {
                   ApprenticeshipId = ApprenticeshipId,
                   AccountId = 222,
                   UserInfo = new UserInfo { UserId = 122.ToString()},
                   Option = apprenticeship.TrainingCourseOption
                }
            };

            editApprenticeshipValidationService = new Mock<IEditApprenticeshipValidationService>();
            currentDateTime = new Mock<ICurrentDateTime>();
            mediator = new Mock<IMediator>();

            mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new GetTrainingProgrammeQueryResult {
                    TrainingProgramme = new Types.TrainingProgramme { Name = "CourseName" , ProgrammeType = Types.ProgrammeType.Standard }
                }));

            mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeVersionQuery>(), It.IsAny<CancellationToken>()))
               .Returns(() => Task.FromResult(new GetTrainingProgrammeVersionQueryResult
               {
                   TrainingProgramme = new Types.TrainingProgramme { Name = "CourseName", ProgrammeType = Types.ProgrammeType.Standard, Version = "1.0", StandardUId = "ST0123_1.0" }
               }));

            Handler = new EditApprenticeshipCommandHandler(editApprenticeshipValidationService.Object,
                lazyProviderDbContext,
                authenticationService.Object,
                currentDateTime.Object,
                mediator.Object,
                Mock.Of<ILogger<EditApprenticeshipCommandHandler>>());
        }
        
        public EditApprenticeshipCommandHandlerTestsFixture SetEmailAddressConfirmedByApprentice()
        {
            var first = Db.Apprenticeships.First();
            first.EmailAddressConfirmed = true;
            Db.SaveChanges();
            return this;
        }

        public EditApprenticeshipCommandHandlerTestsFixture SetParty(Party party)
        {
            Party = party;
            if (party == Party.Provider)
            {
                Command.EditApprenticeshipRequest.ProviderId = 333;
                Command.EditApprenticeshipRequest.AccountId = null;
            }
            if (party == Party.Employer)
            {
                Command.EditApprenticeshipRequest.ProviderId = null;
                Command.EditApprenticeshipRequest.AccountId = 222;
            }

            return this;
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        internal void VerifyOnlyEmployerImmediateUpdate()
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual("NewEmployerRef", apprenticeship.EmployerRef);
            Assert.AreEqual(0, apprenticeship.ApprenticeshipUpdate.Count);
        }

        internal void VerifyOnlyProviderImmediateUpdate()
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual("NewProviderRef", apprenticeship.ProviderRef);
            Assert.AreEqual(0, apprenticeship.ApprenticeshipUpdate.Count);
        }

        internal void VerifyApprenticeshipUpdateCreated(string expectedValue, Func<Apprenticeship, string> getApprenticeshipUpdateValue)
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual(1, apprenticeship.ApprenticeshipUpdate.Count);
            Assert.AreEqual(expectedValue, getApprenticeshipUpdateValue(apprenticeship));
        }

        internal void VerifyApprenticeshipUpdateCreated(DateTime? expectedValue, Func<Apprenticeship, DateTime?> getApprenticeshipUpdateValue)
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual(1, apprenticeship.ApprenticeshipUpdate.Count);
            Assert.AreEqual(expectedValue, getApprenticeshipUpdateValue(apprenticeship));
        }

        internal void VerifyApprenticeshipUpdateCreated(long? expectedValue, Func<Apprenticeship, long?> getApprenticeshipUpdateValue)
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual(1, apprenticeship.ApprenticeshipUpdate.Count);
            Assert.AreEqual(expectedValue, getApprenticeshipUpdateValue(apprenticeship));
        }

        internal void VerifyApprenticeshipUpdateCreated(DeliveryModel? expectedValue, Func<Apprenticeship, DeliveryModel?> getApprenticeshipUpdateValue)
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual(1, apprenticeship.ApprenticeshipUpdate.Count);
            Assert.AreEqual(expectedValue, getApprenticeshipUpdateValue(apprenticeship));
        }

        internal void VerifyEmploymentFieldsAreNull()
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            Assert.AreEqual(1, apprenticeship.ApprenticeshipUpdate.Count);
            Assert.IsNull(apprenticeship.ApprenticeshipUpdate.First().EmploymentEndDate);
            Assert.IsNull(apprenticeship.ApprenticeshipUpdate.First().EmploymentPrice);
        }

        internal void VerifyApprenticeshipUpdateCreatedEvent()
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            var emittedEvent = (ApprenticeshipUpdateCreatedEvent)UnitOfWorkContext.GetEvents().Single(x => x is ApprenticeshipUpdateCreatedEvent);

            Assert.AreEqual(ApprenticeshipId, emittedEvent.ApprenticeshipId);
            Assert.AreEqual(apprenticeship.Cohort.EmployerAccountId, emittedEvent.AccountId);
            Assert.AreEqual(apprenticeship.Cohort.ProviderId, emittedEvent.ProviderId);
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}
