using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture()]
    public class UserInfoValidatorTests
    {
        [TestCase("UserId", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserId_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.UserId, value, expectedValid);
        }

        [TestCase("UserDisplayName", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserName_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.UserDisplayName, value, expectedValid);
        }

        [TestCase("CorrectEmail@here.com", true)]
        [TestCase("IncorrectEmail", false)]
        [TestCase("IncorrectEmail@nowhere", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Validate_UserEmail_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.UserEmail, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<UserInfo, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new UserInfoValidator();

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
