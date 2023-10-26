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
    public class CreateCohortWithOtherPartyRequestValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortWithOtherPartyRequest { AccountId = value };
            
            AssertValidationResult(r => r.AccountId, request, expectedValid);
        }
        
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountLegalEntityId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortWithOtherPartyRequest { AccountLegalEntityId = value };
            
            AssertValidationResult(r => r.AccountLegalEntityId, request, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortWithOtherPartyRequest { ProviderId = value };
            
            AssertValidationResult(r => r.ProviderId, request, expectedValid);
        }

        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("XXXX! XXXXX XXXXX", true)]
        public void Validate_Message_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new CreateCohortWithOtherPartyRequest { Message = value };
            
            AssertValidationResult(r => r.Message, request, expectedValid);
        }

        [Test]
        public void Validate_MessageTooLong_ShouldBeInValid()
        {
            var request = new CreateCohortWithOtherPartyRequest { Message = new string('X',501) };

            AssertValidationResult(r => r.Message, request, false);
        }

        [Test]
        public void Validate_UserInfoIsNull_ShouldBeValid()
        {
            var request = new CreateCohortWithOtherPartyRequest { UserInfo = null };
            
            AssertValidationResult(r => r.UserInfo, request, true);
        }

        [Test]
        public void Validate_UserInfoIsNotNullAndHasGoodData_ShouldBeValid()
        {
            var userInfo = new UserInfo { UserId = "EE", UserDisplayName = "Name", UserEmail = "a@a.com" };
            var request = new CreateCohortWithOtherPartyRequest { UserInfo = userInfo };
            
            AssertValidationResult(r => r.UserInfo, request, true);
        }

        private static void AssertValidationResult<T>(Expression<Func<CreateCohortWithOtherPartyRequest, T>> property, CreateCohortWithOtherPartyRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new CreateCohortWithOtherPartyRequestValidator();

            // Act
            var result = validator.TestValidate(request);
            
            // Assert
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
