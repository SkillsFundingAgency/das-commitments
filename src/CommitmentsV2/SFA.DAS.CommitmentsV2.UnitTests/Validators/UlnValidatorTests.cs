using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class UlnValidatorTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("123456789", false)]
        [TestCase("Ten digits", false)]
        public void Validate_Uln(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.Uln, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<CreateCohortRequest,T>> property,T value, bool expectedValid)
        {
            var validator = new UlnValidator();

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