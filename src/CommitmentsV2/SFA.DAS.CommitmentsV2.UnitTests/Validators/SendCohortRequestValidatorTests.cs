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
            var request = new SendCohortRequest { UserInfo = isSet ? new UserInfo() : null };
            AssertValidationResult(r => r.UserInfo, request, isValid);
        }

        private static void AssertValidationResult<T>(Expression<Func<SendCohortRequest, T>> property, SendCohortRequest request, bool isValid)
        {
            var validator = new SendCohortRequestValidator();

            var result = validator.TestValidate(request);

            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(property);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(property);
            }
        }
    }
}