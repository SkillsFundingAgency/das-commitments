using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class GetCohortsRequestTests
    {
        [TestCase(1, true)]
        [TestCase(null, false)]
        public void Validate_AccountId_ShouldBeValidated(long? value, bool expectedValid)
        {
            AssertValidationResult(request => request.AccountId, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<GetCohortsRequest, T>> property,  T value, bool expectedValid)
        {
            var validator = new GetCohortsRequestValidator();

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