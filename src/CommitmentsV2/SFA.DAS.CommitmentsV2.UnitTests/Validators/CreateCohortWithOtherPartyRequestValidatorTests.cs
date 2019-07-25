using System;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class CreateCohortWithOtherPartyRequestValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountLegalEntityId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.AccountLegalEntityId, value, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.ProviderId, value, expectedValid);
        }

        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("XXXX! XXXXX XXXXX", true)]
        public void Validate_Message_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.Message, value, expectedValid);
        }

        [Test]
        public void Validate_MessageTooLong_ShouldBeInValid()
        {
            var value = new string('X',501);

            AssertValidationResult(request => request.Message, value, false);
        }

        [Test]
        public void Validate_UserInfoIsNull_ShouldBeValid()
        {
            AssertValidationResult(request => request.UserInfo, null, true);
        }

        [Test]
        public void Validate_UserInfoIsNotNullAndHasGoodData_ShouldBeValid()
        {
            var userInfo = new UserInfo { UserId = "EE", UserDisplayName = "Name", UserEmail = "a@a.com" };
            AssertValidationResult(request => request.UserInfo, userInfo, true);
        }

        private void AssertValidationResult<T>(Expression<Func<CreateCohortWithOtherPartyRequest, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new CreateCohortWithOtherPartyRequestValidator(Mock.Of<IAuthorizationService>());

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
