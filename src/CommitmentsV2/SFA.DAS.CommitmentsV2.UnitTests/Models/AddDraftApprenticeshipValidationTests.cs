using System;
using System.Linq;
using Moq;
using NUnit.Framework;
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
    }

    public class AddDraftApprenticeshipValidationTestsFixture
    {
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public Commitment Cohort;
        public ICurrentDateTime CurrentDateTime;

        public AddDraftApprenticeshipValidationTestsFixture()
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails();
            SetupMinimumNameProperties();
            Cohort = new Commitment();
            CurrentDateTime = new CurrentDateTime(new DateTime(2019,04,01,0,0,0, DateTimeKind.Utc));
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
                Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Mock.Of<IUlnValidator>(), CurrentDateTime);
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
            return this;
        }
    }
}