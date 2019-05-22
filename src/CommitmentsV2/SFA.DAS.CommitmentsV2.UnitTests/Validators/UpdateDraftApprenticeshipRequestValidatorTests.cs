using System;
using System.Globalization;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture()]
    public class UpdateDraftApprenticeshipRequestValidatorTests
    {
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_FirstName_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.FirstName, value, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_LastName_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.LastName, value, expectedValid);
        }

        [TestCase("", true)]
        [TestCase("2001-05-01", true)]
        public void Validate_DateOfBirth_ShouldBeValidated(string value, bool expectedValid)
        {
            DateTime? dateOfBirth = string.IsNullOrWhiteSpace(value)
                ? (DateTime?) null
                : DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            AssertValidationResult(request => request.DateOfBirth, dateOfBirth,  expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX50", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_Uln_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.Uln, value, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_CourseCode_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.CourseCode, value, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(null, true)]
        public void Validate_Cost_ShouldBeValidated(int? value, bool expectedValid)
        {
            AssertValidationResult(request => request.Cost, value, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX50", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void Validate_ProviderReference_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.Reference, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<UpdateDraftApprenticeshipRequest, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new UpdateDraftApprenticeshipRequestValidator();

            // Act
            if (expectedValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, value);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, value);
            }
        }
    }
}
