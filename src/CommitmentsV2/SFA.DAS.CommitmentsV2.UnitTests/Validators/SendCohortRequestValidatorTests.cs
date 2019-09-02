using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    [Parallelizable]
    public class SendCohortRequestValidatorTests
    {
        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_UserInfo_ShouldBeValidated(bool isSet, bool isValid)
        {
            AssertValidationResult(r => r.UserInfo, isSet ? new UserInfo() : null, isValid);
        }

        private void AssertValidationResult<T>(Expression<Func<SendCohortRequest, T>> property, T value, bool isValid)
        {
            var validator = new SendCohortRequestValidator();
            
            if (isValid)
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