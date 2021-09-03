using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
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

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class EditApprenticeshipCommandHandlerTests
    {
        EditApprenticeshipCommandHandlerTestsFixture fixture;

        [SetUp]
        public void Setup()
        {
            fixture = new EditApprenticeshipCommandHandlerTestsFixture();
        }

        [Test]
        public async Task OnlyImmediateUpdate_WhenOnlyEmployerReferenceIsChanged()
        {
            fixture.SetParty(Party.Employer);
            fixture.Command.EditApprenticeshipRequest.EmployerReference = "NewEmployerRef";

            await fixture.Handle();
            fixture.VerifyOnlyEmployerImmediateUpdate();
        }

        [Test]
        public async Task OnlyImmediateUpdate_WhenOnlyProviderReferenceIsChanged()
        {
            fixture.SetParty(Party.Provider);
            fixture.Command.EditApprenticeshipRequest.ProviderReference = "NewProviderRef";

            await fixture.Handle();
            fixture.VerifyOnlyProviderImmediateUpdate();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenFirstNameIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.FirstName = "NewFirstName";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated("NewFirstName", app => app.ApprenticeshipUpdate.First().FirstName);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenLastNameIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.LastName = "NewLastName";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated("NewLastName", app => app.ApprenticeshipUpdate.First().LastName);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmailAddressIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.Email = "New@mail.com";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated("New@mail.com", app => app.ApprenticeshipUpdate.First().Email);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEmailAddressCannotBeChangedWhenConfirmationStatusExists(Party party)
        {
            fixture.SetParty(party).SetApprenticeshipConfirmationStatus();
            fixture.Command.EditApprenticeshipRequest.Email = "New@mail.com";

            try
            {
                await fixture.Handle();
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Should()
                    .Be(
                        "Unable to create an ApprenticeshipUpdate with an email change, as the Apprentice has confirmed the email address");
            }
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenDobIsChanged(Party party)

        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.DateOfBirth = DateTime.UtcNow;

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(fixture.Command.EditApprenticeshipRequest.DateOfBirth.Value,
                app => app.ApprenticeshipUpdate.First().DateOfBirth.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenStartDateIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.StartDate = DateTime.UtcNow;
            
            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(fixture.Command.EditApprenticeshipRequest.StartDate.Value,
                app => app.ApprenticeshipUpdate.First().StartDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenEndDateIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.EndDate = DateTime.UtcNow;
         
            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(fixture.Command.EditApprenticeshipRequest.EndDate.Value,
                app => app.ApprenticeshipUpdate.First().EndDate.Value);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenCourseCodeIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated("NewCourse",
                app => app.ApprenticeshipUpdate.First().TrainingCode);
            fixture.VerifyApprenticeshipUpdateCreated("CourseName",
              app => app.ApprenticeshipUpdate.First().TrainingName);
            fixture.VerifyApprenticeshipUpdateCreated((int)Commitments.Api.Types.Apprenticeship.Types.TrainingType.Standard,
            app => (int) app.ApprenticeshipUpdate.First().TrainingType);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task ThenMultipleAreChangedIsChanged(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";
            fixture.Command.EditApprenticeshipRequest.EndDate = DateTime.UtcNow;
            fixture.Command.EditApprenticeshipRequest.LastName = "NewLastName";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated("NewCourse",
                app => app.ApprenticeshipUpdate.First().TrainingCode);
            fixture.VerifyApprenticeshipUpdateCreated(fixture.Command.EditApprenticeshipRequest.EndDate.Value,
                 app => app.ApprenticeshipUpdate.First().EndDate.Value);
            fixture.VerifyApprenticeshipUpdateCreated("NewLastName", app => app.ApprenticeshipUpdate.First().LastName);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task NotChangedFieldsAreNull_InApprenticehipUpdateTable(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().FirstName);
            fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().LastName);
            fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().Cost?.ToString());
            fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().DateOfBirth);
            fixture.VerifyApprenticeshipUpdateCreated(null,
               app => app.ApprenticeshipUpdate.First().StartDate);
            fixture.VerifyApprenticeshipUpdateCreated(null,
               app => app.ApprenticeshipUpdate.First().EndDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task Then_OriginatorIsSetTo_PartyMakingTheChange(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated( party == Party.Employer ? (int)Originator.Employer : (int)Originator.Provider,
                app => (int)app.ApprenticeshipUpdate.First().Originator);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task UpdateOriginIsSetTo_ChangeOfCircumstances(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated((int)Types.ApprenticeshipUpdateOrigin.ChangeOfCircumstances,
                app => (int)app.ApprenticeshipUpdate.First().UpdateOrigin);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task EffectiveFromDate_Is_Set_To_Apprenticeship_StartDate(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(fixture.Db.Apprenticeships.Where(x => x.Id == fixture.ApprenticeshipId).First().StartDate,
                app => app.ApprenticeshipUpdate.First().EffectiveFromDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task EffectiveToDate_Is_Set_To_Null(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(null,
                app => app.ApprenticeshipUpdate.First().EffectiveToDate);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task CreatedOn_Is_Set_To_DateTimeNow(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            var dateTimeNow = DateTime.Now;
            fixture.currentDateTime.Setup(x => x.UtcNow).Returns(dateTimeNow);

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreated(dateTimeNow,
                app => app.ApprenticeshipUpdate.First().CreatedOn);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public async Task Published_ApprenticeshipUpdateCreatedEvent(Party party)
        {
            fixture.SetParty(party);
            fixture.Command.EditApprenticeshipRequest.CourseCode = "NewCourse";

            await fixture.Handle();
            fixture.VerifyApprenticeshipUpdateCreatedEvent();
        }
    }

    public class EditApprenticeshipCommandHandlerTestsFixture
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
                                                 .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
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
                   UserInfo = new UserInfo { UserId = 122.ToString()}
                }
            };

            editApprenticeshipValidationService = new Mock<IEditApprenticeshipValidationService>();
            currentDateTime = new Mock<ICurrentDateTime>();
            mediator = new Mock<IMediator>();

            mediator.Setup(x => x.Send(It.IsAny<GetTrainingProgrammeQuery>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new GetTrainingProgrammeQueryResult {
                    TrainingProgramme = new Types.TrainingProgramme { Name = "CourseName" , ProgrammeType = Types.ProgrammeType.Standard }
                }));

            Handler = new EditApprenticeshipCommandHandler(editApprenticeshipValidationService.Object,
                lazyProviderDbContext,
                authenticationService.Object,
                currentDateTime.Object,
                mediator.Object,
                Mock.Of<ILogger<EditApprenticeshipCommandHandler>>());
        }

        public EditApprenticeshipCommandHandlerTestsFixture SetApprenticeshipConfirmationStatus()
        {
            var fixture = new Fixture();
            var confirmationStatus = fixture.Build<ApprenticeshipConfirmationStatus>()
                .Without(x=>x.Apprenticeship)
                .With(x => x.ApprenticeshipId, ApprenticeshipId).Create();
            Db.ApprenticeshipConfirmationStatus.Add(confirmationStatus);
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

        internal void VerifyApprenticeshipUpdateCreatedEvent()
        {
            var apprenticeship = Db.Apprenticeships.Where(x => x.Id == Command.EditApprenticeshipRequest.ApprenticeshipId).First();
            var emittedEvent = (ApprenticeshipUpdateCreatedEvent)UnitOfWorkContext.GetEvents().Single(x => x is ApprenticeshipUpdateCreatedEvent);

            Assert.AreEqual(ApprenticeshipId, emittedEvent.ApprenticeshipId);
            Assert.AreEqual(apprenticeship.Cohort.EmployerAccountId, emittedEvent.AccountId);
            Assert.AreEqual(apprenticeship.Cohort.ProviderId, emittedEvent.ProviderId);
        }
    }
}
