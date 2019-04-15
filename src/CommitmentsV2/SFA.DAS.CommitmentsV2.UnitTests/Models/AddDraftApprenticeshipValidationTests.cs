using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

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

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("  ", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("Fred", true)]
        public void FirstName_CheckValidation(string firstName, bool passes)
        {
            _fixture.AssertValidationForProperty( () => _fixture.DraftApprenticeshipDetails.FirstName = firstName,
             nameof(_fixture.DraftApprenticeshipDetails.FirstName), 
             passes);
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("  ", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("West", true)]
        public void LastName_CheckValidation(string lastName, bool passes)
        {
            _fixture.AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.LastName = lastName,
                nameof(_fixture.DraftApprenticeshipDetails.LastName),
                passes);
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

        [TestCase(null, true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        [TestCase("Provider", true)]
        public void ProviderRef_CheckValidation(string @ref, bool passes)
        {
            _fixture.WithProviderCohort()
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.Reference = @ref,
                nameof(_fixture.DraftApprenticeshipDetails.Reference),
                passes);
        }

        [TestCase(null, true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        [TestCase("Employer", true)]
        public void EmployerRef_CheckValidation(string @ref, bool passes)
        {
            _fixture.WithEmployerCohort()
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.Reference = @ref,
                nameof(_fixture.DraftApprenticeshipDetails.Reference),
                passes);
        }

        [TestCase("2019-04-01", null, true, Description = "DoB not specified")]
        [TestCase("2019-04-01", "2004-04-01", true, Description = "Exactly 15 years old")]
        [TestCase("2019-04-01", "2004-04-02", false, Description = "One day prior to 15 years old")]
        [TestCase("2019-04-01", "1904-04-01", false, Description = "Exactly 115 years old")]
        [TestCase("2019-04-01", "1904-04-02", true, Description = "One day prior to 115 years old")]

        public void DateOfBirth_CheckValidation(DateTime currentDate, DateTime? dateOfBirth, bool passes)
        {
            var utcDateOfBirth = dateOfBirth.HasValue ? DateTime.SpecifyKind(dateOfBirth.Value, DateTimeKind.Utc) : default(DateTime?);

            _fixture.WithCurrentDate(currentDate)
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

        [TestCase("2019-04-01", null, true)]
        [TestCase("2019-04-01", "2020-08-01", false, Description = "One day after cut off")]
        [TestCase("2019-04-01", "2020-07-31", true, Description = "Day of cut off (last valid day)")]
        [TestCase("2019-04-01", "2018-01-01", true, Description = "Day in the past")]
        public void StartDate_CheckIsWithinAYearOfEndOfCurrentTeachingYear_Validation(DateTime currentDate, DateTime? startDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithCurrentDate(currentDate)
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

            var domainException = Assert.Throws<DomainException>(() =>
                _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails, Mock.Of<IUlnValidator>(),
                    Mock.Of<ICurrentDateTime>(), Mock.Of<IAcademicYearDateProvider>()
                ));

                var startDateError = domainException.DomainErrors.Single(x =>
                    x.PropertyName == nameof(_fixture.DraftApprenticeshipDetails.StartDate));

                Assert.AreEqual(expectedErrorMessage, startDateError.ErrorMessage);
        }
    }

    public class AddDraftApprenticeshipValidationTestsFixture
    {
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public Commitment Cohort;
        public ICurrentDateTime CurrentDateTime;
        public IAcademicYearDateProvider AcademicYearDateProvider;

        public AddDraftApprenticeshipValidationTestsFixture()
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                TrainingProgramme = new TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue)
            };
            SetupMinimumNameProperties();
            Cohort = new Commitment();
            CurrentDateTime = new CurrentDateTime(new DateTime(2019,04,01,0,0,0, DateTimeKind.Utc));
            AcademicYearDateProvider = new AcademicYearDateProvider(CurrentDateTime);
        }

        public AddDraftApprenticeshipValidationTestsFixture WithProviderCohort()
        {
            Cohort = new Commitment{ EditStatus = EditStatus.ProviderOnly };
            return this;
        }
        public AddDraftApprenticeshipValidationTestsFixture WithEmployerCohort()
        {
            Cohort = new Commitment { EditStatus = EditStatus.EmployerOnly };
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture SetupMinimumNameProperties()
        {
            DraftApprenticeshipDetails.FirstName = "Fred";
            DraftApprenticeshipDetails.LastName = "West";
            return this;
        }

        public void AssertValidationForProperty(Action setup, string propertyName, bool expected)
        {
            setup();

            try
            {
                Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Mock.Of<IUlnValidator>(), CurrentDateTime, AcademicYearDateProvider);
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