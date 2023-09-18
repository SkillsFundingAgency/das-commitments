using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class UserInfoValidatorTests
    {
        [TestCase("UserId", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserId_ShouldBeValidated(string value, bool expectedValid)
        {
            var userInfo = new UserInfo { UserId = value };
            
            AssertValidationResult(request => request.UserId, userInfo, expectedValid);
        }

        [TestCase("UserDisplayName", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserName_ShouldBeValidated(string value, bool expectedValid)
        {
            var userInfo = new UserInfo { UserDisplayName = value };
            
            AssertValidationResult(request => request.UserDisplayName, userInfo, expectedValid);
        }

        [TestCase("CorrectEmail@here.com", true)]
        [TestCase("IncorrectEmail", false)]
        [TestCase("IncorrectEmail@nowhere", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserEmail_ShouldBeValidated(string value, bool expectedValid)
        {
            var userInfo = new UserInfo { UserEmail = value };
            
            AssertValidationResult(request => request.UserEmail, userInfo, expectedValid);
        }

        private static void AssertValidationResult<T>(Expression<Func<UserInfo, T>> property, UserInfo userInfo, bool expectedValid)
        {
            // Arrange
            var validator = new UserInfoValidator();

            // Act
            var result = validator.TestValidate(userInfo);
            
            // assert
            if (expectedValid)
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