using System;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

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
        [TestCase(0, true)]
        [TestCase(100000, true)]
        [TestCase(100001, false)]
        public void Cost_CheckValidation(decimal? cost, bool passes)
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
            _fixture.AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.ProviderRef = @ref,
                nameof(_fixture.DraftApprenticeshipDetails.ProviderRef),
                passes);
        }

        [TestCase(null, true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        [TestCase("Employer", true)]
        public void EmployerRef_CheckValidation(string @ref, bool passes)
        {
            _fixture.AssertValidationForProperty(() => _fixture.DraftApprenticeshipDetails.EmployerRef = @ref,
                nameof(_fixture.DraftApprenticeshipDetails.EmployerRef),
                passes);
        }

    }

    public class AddDraftApprenticeshipValidationTestsFixture
    {
        public DraftApprenticeshipDetails DraftApprenticeshipDetails;
        public Commitment Cohort;

        public AddDraftApprenticeshipValidationTestsFixture()
        {
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails();
            Cohort = new Commitment();
            SetupMinimumNameProperties();
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
                Cohort.AddDraftApprenticeship(DraftApprenticeshipDetails, Mock.Of<IUlnValidator>());
                Assert.AreEqual(expected, true);
            }
            catch (DomainException ex)
            {
                Assert.AreEqual(expected, false);
                Assert.Contains(propertyName, ex.DomainErrors.Select(x => x.PropertyName).ToList());
            }
        }
    }
}