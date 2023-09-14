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
    public class ApproveCohortRequestValidatorTests
    {
        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_UserInfo_ShouldBeValidated(bool isSet, bool isValid)
        {
            var request = new ApproveCohortRequest { UserInfo = isSet ? new UserInfo() : null };
            AssertValidationResult(r => r.UserInfo, request, isValid);
        }

        private static void AssertValidationResult<T>(Expression<Func<ApproveCohortRequest, T>> property, ApproveCohortRequest request, bool isValid)
        {
            var validator = new ApproveCohortRequestValidator();
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