using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services.Shared;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipValidationTests
    {
        private AddDraftApprenticeshipValidationTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddDraftApprenticeshipValidationTestsFixture().WithProviderCohort();
        }

        [TestCase(null, null, true)]
        [TestCase("2022-01-20", null, true)]
        [TestCase("2022-01-20", "2022-02-22", false)]
        [TestCase("2022-01-20", "2022-01-20", false)]
        [TestCase("2001-01-01", null, false)]
        public void EndDate_CheckValidation(string endDateString, string startDateString, bool passes)
        {
            DateTime? endDate = endDateString == null ? (DateTime?) null : DateTime.Parse(endDateString);
            DateTime? startDate = startDateString == null ? (DateTime?) null : DateTime.Parse(startDateString);


            _fixture.AssertValidationForProperty(() =>
                {
                    _fixture.DraftApprenticeshipDetails.EndDate = endDate;
                    _fixture.DraftApprenticeshipDetails.StartDate = startDate;
                },
                nameof(_fixture.DraftApprenticeshipDetails.EndDate),
                passes);
        }

        [TestCase(null, true)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(100000, true)]
        [TestCase(100001, false)]
        public void Cost_CheckValidation(int? cost, bool passes)
        {
            _fixture.AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.Cost = cost,
                nameof(_fixture.DraftApprenticeshipDetails.Cost),
                passes);
        }

        [TestCase("2019-04-01", null, true, Description = "DoB not specified")]
        [TestCase("2019-04-01", "2004-04-01", true, Description = "Exactly 15 years old")]
        [TestCase("2019-04-01", "2004-04-02", false, Description = "One day prior to 15 years old")]
        [TestCase("2019-04-01", "1904-04-01", false, Description = "Exactly 115 years old")]
        [TestCase("2019-04-01", "1904-04-02", true, Description = "One day prior to 115 years old")]
        [TestCase(null, "1899-12-31", false, Description = "Date earlier than minimum acceptable")]
        public void DateOfBirth_Validation(DateTime? courseStartDate, DateTime? dateOfBirth, bool passes)
        {
            var utcDateOfBirth = dateOfBirth.HasValue
                ? DateTime.SpecifyKind(dateOfBirth.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithStartDate(courseStartDate)
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.DateOfBirth = utcDateOfBirth,
                    nameof(_fixture.DraftApprenticeshipDetails.DateOfBirth),
                    passes);
        }

        [TestCase(null, true)]
        [TestCase("2017-04-30", false)]
        [TestCase("2017-05-01", true)]
        public void StartDate_CheckNotBeforeMay2017_Validation(DateTime? startDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithCurrentDate(new DateTime(2017, 5, 1))
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.StartDate = utcStartDate,
                    nameof(_fixture.DraftApprenticeshipDetails.StartDate)
                    , passes);
        }

        [TestCase(null, "2019-01-01", "2019-12-31", true, Description = "Start date not specified")]
        [TestCase("2019-06-01", "2019-01-01", "2019-12-31", true, Description = "Active")]
        [TestCase("2018-06-01", "2019-01-01", "2019-12-31", false, Description = "Pending")]
        [TestCase("2020-01-01", "2019-01-01", "2019-12-31", false, Description = "Expired")]
        [TestCase("2020-01-01", "2000-01-01", "2019-12-31", false, Description =
            "Expired but course effective from before DAS")]
        public void StartDate_CheckTrainingProgrammeActive_Validation(DateTime? startDate,
            DateTime courseEffectiveFromDate, DateTime courseEffectiveToDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithTrainingProgrammeEffectiveBetween(courseEffectiveFromDate, courseEffectiveToDate)
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.StartDate = utcStartDate,
                    nameof(_fixture.DraftApprenticeshipDetails.StartDate),
                    passes);
        }

        [TestCase("2015-08-01", "The start date must not be earlier than May 2017", Description =
            "Course effective before DAS")]
        [TestCase("2018-08-01", "This training course is only available to apprentices with a start date after 07 2018",
            Description = "Course effective after DAS")]
        public void StartDate_CheckTrainingProgrammeActive_BeforeOrAfterDas_Validation(DateTime courseEffectiveFromDate,
            string expectedErrorMessage)
        {

            _fixture.DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                StartDate = new DateTime(1950, 01, 01),
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, courseEffectiveFromDate, courseEffectiveFromDate.AddYears(1))
            };

            var domainException = Assert.Throws<DomainException>(() => _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails, Party.Provider,
                    _fixture.UserInfo));

            var startDateError = domainException.DomainErrors.Single(x => x.PropertyName == nameof(_fixture.DraftApprenticeshipDetails.StartDate));

            Assert.AreEqual(expectedErrorMessage, startDateError.ErrorMessage);
        }

        [TestCase(Party.Provider, Party.Employer)]
        [TestCase(Party.Employer, Party.Provider)]
        [TestCase(Party.None, Party.Provider)]
        [TestCase(Party.None, Party.Employer)]
        public void Party_CheckValidation(Party withParty, Party modifyingParty)
        {
            _fixture.Cohort.WithParty = withParty;

            var domainException = Assert.Throws<DomainException>(() => _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails, modifyingParty, _fixture.UserInfo));
            var domainError = domainException.DomainErrors.SingleOrDefault(e => e.PropertyName == nameof(_fixture.Cohort.WithParty));

            Assert.AreEqual($"Cohort must be with the party; {modifyingParty} is not valid", domainError?.ErrorMessage);
        }

        [TestCase(1, "", "", true)]
        [TestCase(1, "AAA111", "AAA111", false)]
        [TestCase(1, "AAA111", "BBB222", true)]
        public void Uln_CheckNoDuplicates_Validation(long existingId, string existingUln, string uln, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithApprenticeship(existingId, existingUln).WithUln(uln),
                nameof(_fixture.DraftApprenticeshipDetails.Uln),
                passes);
        }

        [TestCase(null,  true)]
        [TestCase("valid@email.com",  true)]
        [TestCase("valid@email",  false)]
        [TestCase("invalidemail@", false)]
        [TestCase("invalidemail.com",  false)]
        [TestCase("invalid\\@email.com", false)]
        [TestCase("invalid@email.com;valid@email.com", false)]
        public void Email_CheckEmailIsValidIfPresent_Validation(string email, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithEmail(email),
                nameof(_fixture.DraftApprenticeshipDetails.Email),
                passes);
        }

        [TestCase("!", true)]
        [TestCase("#", true)]
        [TestCase("$", true)]
        [TestCase("%", true)]
        [TestCase("&", true)]
        [TestCase("'", true)]
        [TestCase("`", true)]
        [TestCase("*", true)]
        [TestCase("+", true)]
        [TestCase("-", true)]
        [TestCase("/", true)]
        [TestCase("=", true)]
        [TestCase("?", true)]
        [TestCase("^", true)]
        [TestCase("_", true)]
        [TestCase("{", true)]
        [TestCase("|", true)]
        [TestCase("}", true)]
        [TestCase("~", true)]
        [TestCase(" ", false)]
        [TestCase("[", false)]
        [TestCase("]", false)]
        [TestCase("(", false)]
        [TestCase(")", false)]
        [TestCase(";", false)]
        public void Email_CheckLocalEmailCharacterIfPresent_Validation(string emailChar, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithEmail($"a{emailChar}email@test.com"),
                nameof(_fixture.DraftApprenticeshipDetails.Email),
                passes);
        }

        [TestCase("!", false)]
        [TestCase("#", false)]
        [TestCase("$", false)]
        [TestCase("%", false)]
        [TestCase("&", false)]
        [TestCase("'", false)]
        [TestCase("`", false)]
        [TestCase("*", false)]
        [TestCase("+", false)]
        [TestCase("-", true)]
        [TestCase("/", false)]
        [TestCase("=", false)]
        [TestCase("?", false)]
        [TestCase("^", false)]
        [TestCase("_", true)]
        [TestCase("{", false)]
        [TestCase("|", false)]
        [TestCase("}", false)]
        [TestCase("~", false)]
        [TestCase(" ", false)]
        [TestCase("[", false)]
        [TestCase("]", false)]
        [TestCase("(", false)]
        [TestCase(")", false)]
        [TestCase(";", false)]
        public void Email_CheckDomainEmailCharacterIfPresent_Validation(string emailChar, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithEmail($"aemail@te{emailChar}st.com"),
                nameof(_fixture.DraftApprenticeshipDetails.Email),
                passes);
        }
    }

    public class AddDraftApprenticeshipValidationTestsFixture
    {
        public UnitOfWorkContext UnitOfWorkContext;
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public CommitmentsV2.Models.Cohort Cohort;
        public ICurrentDateTime CurrentDateTime;
        public IAcademicYearDateProvider AcademicYearDateProvider;
        public UserInfo UserInfo { get; }

        public AddDraftApprenticeshipValidationTestsFixture()
        {
            var autoFixture = new Fixture();
            UnitOfWorkContext = new UnitOfWorkContext();
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue)
            };
            SetupMinimumNameProperties();
            Cohort = new CommitmentsV2.Models.Cohort {EditStatus = EditStatus.ProviderOnly};
            CurrentDateTime = new CurrentDateTime(new DateTime(2019, 04, 01, 0, 0, 0, DateTimeKind.Utc));
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
            UserInfo = autoFixture.Create<UserInfo>();
        }

        public AddDraftApprenticeshipValidationTestsFixture WithProviderCohort()
        {
            Cohort = new CommitmentsV2.Models.Cohort {WithParty = Party.Provider, ProviderId = 1};
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithEmployerCohort()
        {
            Cohort = new CommitmentsV2.Models.Cohort {WithParty = Party.Employer};
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture SetupMinimumNameProperties()
        {
            DraftApprenticeshipDetails.FirstName = "TestFirstName";
            DraftApprenticeshipDetails.LastName = "TestLastName";
            return this;
        }

        public void AssertValidationForProperty(Action setup, string propertyName, bool expected)
        {
            setup();

            try
            {
                Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Party.Provider, UserInfo);
                Assert.AreEqual(expected, true);
            }
            catch (DomainException ex)
            {
                Assert.AreEqual(expected, false);
                Assert.Contains(propertyName, ex.DomainErrors.Select(x => x.PropertyName).ToList());
            }
        }

        public AddDraftApprenticeshipValidationTestsFixture WithCurrentDate(DateTime currentDate)
        {
            var utcCurrentDate = DateTime.SpecifyKind(currentDate, DateTimeKind.Utc);
            CurrentDateTime = new CurrentDateTime(utcCurrentDate);
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithStartDate(DateTime? startDate)
        {
            DraftApprenticeshipDetails.StartDate = startDate;
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithEmail(string email)
        {
            DraftApprenticeshipDetails.Email = email;
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithTrainingProgrammeEffectiveBetween(DateTime startDate, DateTime endDate)
        {
            DraftApprenticeshipDetails.TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST",
                "TEST",
                ProgrammeType.Framework,
                DateTime.SpecifyKind(startDate,DateTimeKind.Utc),
                DateTime.SpecifyKind(endDate,DateTimeKind.Utc));
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithApprenticeship(long id, string uln)
        {
            var draftApprenticeshipDetails = new DraftApprenticeshipDetails().Set(d => d.Uln, uln);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, Party.Provider).Set(d => d.Id, id);
            
            Cohort.Apprenticeships.Add(draftApprenticeship);
            
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithUln(string uln)
        {
            DraftApprenticeshipDetails.Uln = uln;
            
            return this;
        }
    }
}