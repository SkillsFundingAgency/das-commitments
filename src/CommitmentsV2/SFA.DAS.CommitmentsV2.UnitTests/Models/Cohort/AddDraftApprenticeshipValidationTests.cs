using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
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

        [Test]
        public void IfEndDateIsLessThan365DaysAfterStartDateForAPilotApprenticeshipValidationFails()
        {
            var endDate = new DateTime(2023, 12, 1);
            var assumedEndDate = new DateTime(2023, 12, 31);
            var startDate = new DateTime(2023, 1, 2);

            _fixture.AssertValidationForProperty(() =>
                {
                    _fixture.DraftApprenticeshipDetails.IsOnFlexiPaymentPilot = true;
                    _fixture.DraftApprenticeshipDetails.EndDate = endDate;
                    _fixture.DraftApprenticeshipDetails.ActualStartDate = startDate;
                },
                nameof(_fixture.DraftApprenticeshipDetails.EndDate),
                false);
        }

        [TestCase(365)]
        [TestCase(366)]
        public void IfEndDateIs365DaysAfterStartDateForAPilotApprenticeshipValidationPasses(int daysAfterStartDate)
        {
            var endDate = new DateTime(2023, 12, 31);
            var startDate = endDate.AddDays(-(daysAfterStartDate - 1));

            _fixture.AssertValidationForProperty(() =>
                {
                    _fixture.DraftApprenticeshipDetails.IsOnFlexiPaymentPilot = true;
                    _fixture.DraftApprenticeshipDetails.EndDate = endDate;
                    _fixture.DraftApprenticeshipDetails.ActualStartDate = startDate;
                },
                nameof(_fixture.DraftApprenticeshipDetails.EndDate),
                true);
        }

        [Test]
        public void IfEndDateIsMoreThan10YearsAfterStartDateForAPilotApprenticeshipValidationFails()
        {
            var endDate = new DateTime(2032, 01, 31);
            var startDate = new DateTime(2022, 01, 31);

            _fixture.AssertValidationForProperty(() =>
                {
                    _fixture.DraftApprenticeshipDetails.IsOnFlexiPaymentPilot = true;
                    _fixture.DraftApprenticeshipDetails.EndDate = endDate;
                    _fixture.DraftApprenticeshipDetails.ActualStartDate = startDate;
                },
                nameof(_fixture.DraftApprenticeshipDetails.EndDate),
                false);
        }

        [TestCase(365)]
        [TestCase(366)]
        public void IfEndDateIsLessThan10YearsAfterStartDateForAPilotApprenticeshipValidationPasses(int daysAfterStartDate)
        {
            var endDate = new DateTime(2032, 01, 1);
            var assumedEndDate = new DateTime(2032, 1, 31);
            var startDate = new DateTime(2022, 02, 1);

            _fixture.AssertValidationForProperty(() =>
                {
                    _fixture.DraftApprenticeshipDetails.IsOnFlexiPaymentPilot = true;
                    _fixture.DraftApprenticeshipDetails.EndDate = endDate;
                    _fixture.DraftApprenticeshipDetails.ActualStartDate = startDate;
                },
                nameof(_fixture.DraftApprenticeshipDetails.EndDate),
                true);
        }

        [TestCase(null, true)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(100000, true)]
        [TestCase(100001, false)]
        public void Cost_CheckValidation(int? cost, bool passes)
        {
            _fixture.AssertValidationForProperty(() => _fixture.WithCost(cost),
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

            Assert.That(startDateError.ErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [TestCase(null, true)]
        [TestCase("2017-04-30", false)]
        [TestCase("2017-05-01", true)]
        public void ActualStartDate_CheckNotBeforeMay2017_Validation(DateTime? startDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithCurrentDate(new DateTime(2017, 5, 1))
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.ActualStartDate = utcStartDate,
                    nameof(_fixture.DraftApprenticeshipDetails.ActualStartDate)
                    , passes);
        }

        [TestCase(null, "2019-01-01", "2019-12-31", true, Description = "Start date not specified")]
        [TestCase("2019-06-01", "2019-01-01", "2019-12-31", true, Description = "Active")]
        [TestCase("2018-06-01", "2019-01-01", "2019-12-31", false, Description = "Pending")]
        [TestCase("2020-01-01", "2019-01-01", "2019-12-31", false, Description = "Expired")]
        [TestCase("2020-01-01", "2000-01-01", "2019-12-31", false, Description =
            "Expired but course effective from before DAS")]
        public void ActualStartDate_CheckTrainingProgrammeActive_Validation(DateTime? startDate,
            DateTime courseEffectiveFromDate, DateTime courseEffectiveToDate, bool passes)
        {
            var utcStartDate = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : default(DateTime?);

            _fixture.WithTrainingProgrammeEffectiveBetween(courseEffectiveFromDate, courseEffectiveToDate)
                .AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.ActualStartDate = utcStartDate,
                    nameof(_fixture.DraftApprenticeshipDetails.ActualStartDate),
                    passes);
        }

        [TestCase("2015-08-01", "The start date must not be earlier than May 2017", Description =
            "Course effective before DAS")]
        [TestCase("2018-08-01", "This training course is only available to apprentices with a start date after 07 2018",
            Description = "Course effective after DAS")]
        public void ActualStartDate_CheckTrainingProgrammeActive_BeforeOrAfterDas_Validation(DateTime courseEffectiveFromDate,
            string expectedErrorMessage)
        {

            _fixture.DraftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                ActualStartDate = new DateTime(1950, 01, 01),
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, courseEffectiveFromDate, courseEffectiveFromDate.AddYears(1))
            };

            var domainException = Assert.Throws<DomainException>(() => _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails, Party.Provider,
                    _fixture.UserInfo));

            var startDateError = domainException.DomainErrors.Single(x => x.PropertyName == nameof(_fixture.DraftApprenticeshipDetails.ActualStartDate));

            Assert.That(startDateError.ErrorMessage, Is.EqualTo(expectedErrorMessage));
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

            Assert.That(domainError?.ErrorMessage, Is.EqualTo($"Cohort must be with the party; {modifyingParty} is not valid"));
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

        [TestCase(null, false)]
        [TestCase(DeliveryModel.Regular, true)]
        [TestCase(DeliveryModel.PortableFlexiJob, true)]
        public void DeliveryModel_CheckDeliveryModelIsPresent_Validation(DeliveryModel? dm, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithDeliveryModel(dm),
                nameof(_fixture.DraftApprenticeshipDetails.DeliveryModel),
                passes);
        }

        [TestCase(DeliveryModel.Regular, null, true)]
        [TestCase(DeliveryModel.PortableFlexiJob, null, true)]
        [TestCase(DeliveryModel.PortableFlexiJob, -1, false)]
        [TestCase(DeliveryModel.PortableFlexiJob, 0, false)]
        [TestCase(DeliveryModel.PortableFlexiJob, 1, true)]
        [TestCase(DeliveryModel.PortableFlexiJob, 100000, true)]
        [TestCase(DeliveryModel.PortableFlexiJob, 100001, false)]
        public void EmploymentPrice_CheckEmploymentPriceIsPresentWhenFlexible_Validation(DeliveryModel deliveryModel, int? price, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture.WithDeliveryModel(deliveryModel).WithEmploymentPrice(price),
                nameof(_fixture.DraftApprenticeshipDetails.EmploymentPrice),
                passes);
        }

        [TestCase(1, null, true)]
        [TestCase(2, 1, false)]
        [TestCase(2, 2, true)]
        [TestCase(1, 2, true)]
        public void EmploymentPrice_CheckEmploymentPriceIsLessThanTotalAgreenApprenticeshipPrice_Validation(int? employmentPrice, int? totalPrice, bool passes)
        {
            _fixture.AssertValidationForProperty(
                () =>_fixture
                      .WithDeliveryModel(DeliveryModel.PortableFlexiJob)
                      .WithEmploymentPrice(employmentPrice)
                      .WithCost(totalPrice),
                nameof(_fixture.DraftApprenticeshipDetails.EmploymentPrice),
                passes);
        }

        [TestCase(DeliveryModel.Regular, null)]
        [TestCase(DeliveryModel.PortableFlexiJob, null)]
        [TestCase(DeliveryModel.PortableFlexiJob, "2019-06-01")]
        public void EmploymentEndDate_CheckEmploymentEndDateIsPresent_AllowedValidations(DeliveryModel deliveryModel, string date)
        {
            _fixture.AssertValidationForProperty(
                () => _fixture
                      .WithDeliveryModel(deliveryModel)
                      .WithEmploymentEndDate(TryParseNullableDateTime(date)),
                nameof(_fixture.DraftApprenticeshipDetails.EmploymentPrice),
                true);
        }

        [TestCase(null, null, null, true)]
        [TestCase("2019-06-05", "2019-11-05", null, true)]
        [TestCase("2019-06-05", "2019-11-01", "2019-06-01", false)]
        [TestCase("2019-06-05", "2019-11-05", "2019-06-05", false)]
        [TestCase("2019-06-05", "2019-11-05", "2019-09-04", false)]
        [TestCase("2019-06-05", "2019-11-05", "2019-09-05", true)]
        [TestCase("2019-06-05", "2019-11-05", "2019-11-05", true)]
        [TestCase("2019-06-05", "2019-11-05", "2019-11-06", false)]
        public void EmploymentEndDate_CheckEmploymentEndDate_Validation(string trainingStartDate, string trainingEndDate, string employmentEndDate, bool passes)
        {
            var startDate = TryParseNullableDateTime(trainingStartDate);
            var endDate = TryParseNullableDateTime(trainingEndDate);
            var employmentDate = TryParseNullableDateTime(employmentEndDate);

            _fixture.AssertValidationForProperty(
                () => _fixture
                      .WithDeliveryModel(DeliveryModel.PortableFlexiJob)
                      .WithStartDate(startDate)
                      .WithEndDate(endDate)
                      .WithEmploymentEndDate(employmentDate),
                nameof(_fixture.DraftApprenticeshipDetails.EmploymentEndDate),
                passes);
        }

        [Test]
        public void EmploymentEndDate_CheckEmploymentEndDate_Validation2()
        {
            var startDate = TryParseNullableDateTime(null);
            var endDate = TryParseNullableDateTime(null);
            var employmentDate = TryParseNullableDateTime(null);

            startDate.Should().BeNull();
            endDate.Should().BeNull();
            employmentDate.Should().BeNull();

            _fixture
                .WithDeliveryModel(DeliveryModel.PortableFlexiJob)
                .WithStartDate(startDate)
                .WithEndDate(endDate)
                .WithEmploymentEndDate(employmentDate);

            try
            {
                _fixture.Cohort.AddDraftApprenticeship(_fixture.DraftApprenticeshipDetails, Party.Provider, _fixture.UserInfo);
            }
            catch (DomainException ex)
            {
                ex.DomainErrors.Select(x => x.PropertyName).Should().Contain(nameof(_fixture.DraftApprenticeshipDetails.EmploymentPrice));
            }
        }

        DateTime? TryParseNullableDateTime(string date)
        {
            return DateTime.TryParse(date, out var parsed)
                ? parsed
                : (DateTime?)null;
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
                TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Framework, DateTime.MinValue, DateTime.MaxValue),
                DeliveryModel = DeliveryModel.Regular,
                IsOnFlexiPaymentPilot = false
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
                Assert.That(expected, Is.True);
            }
            catch (DomainException ex)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(expected, Is.False);
                    Assert.That(ex.DomainErrors.Select(x => x.PropertyName).ToList(), Does.Contain(propertyName));
                });
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

        public AddDraftApprenticeshipValidationTestsFixture WithEndDate(DateTime? startDate)
        {
            DraftApprenticeshipDetails.EndDate = startDate;
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithEmail(string email)
        {
            DraftApprenticeshipDetails.Email = email;
            return this;
        }

        public AddDraftApprenticeshipValidationTestsFixture WithDeliveryModel(DeliveryModel? dm)
        {
            DraftApprenticeshipDetails.DeliveryModel = dm;
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

        internal AddDraftApprenticeshipValidationTestsFixture WithEmploymentPrice(int? price)
        {
            DraftApprenticeshipDetails.EmploymentPrice = price;
            return this;
        }

        internal AddDraftApprenticeshipValidationTestsFixture WithEmploymentEndDate(DateTime? date)
        {
            DraftApprenticeshipDetails.EmploymentEndDate = date;
            return this;
        }

        internal AddDraftApprenticeshipValidationTestsFixture WithCost(int? cost)
        {
            DraftApprenticeshipDetails.Cost = cost;
            return this;
        }
    }
}