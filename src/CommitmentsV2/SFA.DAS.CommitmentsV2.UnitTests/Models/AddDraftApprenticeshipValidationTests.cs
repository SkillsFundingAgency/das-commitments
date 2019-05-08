using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipValidationTests
    {
        private AddDraftApprenticeshipValidationTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddDraftApprenticeshipValidationTestsFixture();
        }

        [TestCase(null, null, true)]
        [TestCase("2022-01-20", null, true)]
        [TestCase("2022-01-20", "2022-01-22", false)]
        [TestCase("2001-01-01", null, false)]
        public void EndDate_CheckValidation(string endDateString, string startDateString, bool passes)
        {
            DateTime? endDate = endDateString == null ? (DateTime?)null : DateTime.Parse(endDateString);
            DateTime? startDate = startDateString == null ? (DateTime?)null : DateTime.Parse(startDateString);


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
        public void DateOfBirth_CheckValidation(DateTime courseStartDate, DateTime? dateOfBirth, bool passes)
        {
            var utcDateOfBirth = dateOfBirth.HasValue ? DateTime.SpecifyKind(dateOfBirth.Value, DateTimeKind.Utc) : default(DateTime?);

            _fixture.WithStartDate(courseStartDate)
                .WithCurrentDate(courseStartDate.AddMonths(1))
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

        [TestCase(null, "2019-01-01", "2019-12-31", true,  Description ="Start date not specified")]
        [TestCase("2019-06-01", "2019-01-01", "2019-12-31", true, Description = "Active")]
        [TestCase("2018-06-01", "2019-01-01", "2019-12-31", false, Description ="Pending")]
        [TestCase("2020-01-01", "2019-01-01", "2019-12-31", false, Description ="Expired")]
        [TestCase("2020-01-01", "2000-01-01", "2019-12-31", false, Description = "Expired but course effective from before DAS")]
        public void StartDate_CheckTrainingProgrammeActive_Validation(DateTime? startDate, DateTime courseEffectiveFromDate, DateTime courseEffectiveToDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
             ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
             : default(DateTime?);

            _fixture.WithTrainingProgrammeEffectiveBetween(courseEffectiveFromDate, courseEffectiveToDate)
                .AssertValidationForProperty(()=> _fixture.DraftApprenticeshipDetails.StartDate = utcStartDate,
                    nameof(_fixture.DraftApprenticeshipDetails.StartDate),
                    passes);
        }

        [TestCase("2015-08-01", "The start date must not be earlier than May 2017", Description = "Course effective before DAS" )]
        [TestCase("2018-08-01", "This training course is only available to apprentices with a start date after 07 2018", Description = "Course effective after DAS")]
        public void StartDate_CheckTrainingProgrammeActive_BeforeOrAfterDas_Validation(DateTime courseEffectiveFromDate, string expectedErrorMessage)
        {
            _fixture.DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                StartDate = new DateTime(1950, 01, 01),
                TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, courseEffectiveFromDate, courseEffectiveFromDate.AddYears(1))
            };

            var domainException = Assert.Throws<DomainException>(() => _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails));

                var startDateError = domainException.DomainErrors.Single(x =>
                    x.PropertyName == nameof(_fixture.DraftApprenticeshipDetails.StartDate));

                Assert.AreEqual(expectedErrorMessage, startDateError.ErrorMessage);
        }
    }

    public class AddDraftApprenticeshipValidationTestsFixture
    {
        public UnitOfWorkContext UnitOfWorkContext;
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public Cohort Cohort;
        public ICurrentDateTime CurrentDateTime;
        public IAcademicYearDateProvider AcademicYearDateProvider;

        public AddDraftApprenticeshipValidationTestsFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue)
            };
            SetupMinimumNameProperties();
            Cohort = new Cohort();
            CurrentDateTime = new CurrentDateTime(new DateTime(2019,04,01,0,0,0, DateTimeKind.Utc));
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
        }

        public AddDraftApprenticeshipValidationTestsFixture WithProviderCohort()
        {
            Cohort = new Cohort{ EditStatus = EditStatus.ProviderOnly };
            return this;
        }
        public AddDraftApprenticeshipValidationTestsFixture WithEmployerCohort()
        {
            Cohort = new Cohort { EditStatus = EditStatus.EmployerOnly };
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
                Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails);
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

        public AddDraftApprenticeshipValidationTestsFixture WithStartDate(DateTime startDate)
        {
            DraftApprenticeshipDetails.StartDate = startDate;
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithTrainingProgrammeEffectiveBetween(DateTime startDate, DateTime endDate)
        {
            DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme("TEST",
                "TEST",
                ProgrammeType.Framework,
                DateTime.SpecifyKind(startDate,DateTimeKind.Utc),
                DateTime.SpecifyKind(endDate,DateTimeKind.Utc));
            return this;
        }
    }
}