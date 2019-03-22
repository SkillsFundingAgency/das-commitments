using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture()]
    public class NameValidatorTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Fred", true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX10", false)]
        public void Validate_Firstname(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.FirstName, value, expectedValid);
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Flintstone", true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX10", false)]
        public void Validate_Lastname(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.LastName, value, expectedValid);
        }


        private void AssertValidationResult<T>(Expression<Func<CreateCohortRequest,T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new NameValidator();

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
