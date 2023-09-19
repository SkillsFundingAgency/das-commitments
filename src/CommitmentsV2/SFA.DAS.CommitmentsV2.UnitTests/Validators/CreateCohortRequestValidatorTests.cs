using System;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class CreateCohortRequestValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortRequest { AccountId = value};
            
            AssertValidationResult(r => r.AccountId, request, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_AccountLegalEntityId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortRequest { AccountLegalEntityId = value };
            
            AssertValidationResult(r => r.AccountLegalEntityId, request, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CreateCohortRequest { ProviderId = value };
            
            AssertValidationResult(r => r.ProviderId, request, expectedValid);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_ReservationId_ShouldBeValidated(bool hasValue, bool expectedValid)
        {
            var request = new CreateCohortRequest { ReservationId = hasValue ? Guid.NewGuid() : (Guid?)null };
            
            AssertValidationResult(r => r.ReservationId, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_FirstName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new CreateCohortRequest { FirstName = value };
            
            AssertValidationResult(r => r.FirstName, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_LastName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new CreateCohortRequest { LastName = value };
            
            AssertValidationResult(r => r.LastName, request, expectedValid);
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("X", true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        public void Validate_Ref_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new CreateCohortRequest { OriginatorReference = value };
            
            AssertValidationResult(r => r.OriginatorReference, request, expectedValid);
        }

        [Test]
        public void Validate_UserInfoIsNull_ShouldBeValid()
        {
            var request = new CreateCohortRequest { UserInfo = null };

            AssertValidationResult(r => r.UserInfo, request, true);
         }

        [Test]
        public void Validate_UserInfoIsNotNullAndHasGoodData_ShouldBeValid()
        {
            var request = new CreateCohortRequest { UserInfo = new UserInfo { UserId = "EE", UserDisplayName = "Name", UserEmail = "a@a.com" }};
            
            AssertValidationResult(r => r.UserInfo, request, true);
        }

        [Test]
        public void Validate_IsOnFlexiPaymentPilotIsNull_ShouldBeInvalid()
        {
            var request = new CreateCohortRequest { IsOnFlexiPaymentPilot = null};
            
            AssertValidationResult(r => r.IsOnFlexiPaymentPilot, request, false);
        }

        private static void AssertValidationResult<T>(Expression<Func<CreateCohortRequest, T>> property, CreateCohortRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new CreateCohortRequestValidator();

            // Act
            var result = validator.TestValidate(request);
            
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
