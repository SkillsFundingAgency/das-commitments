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
    public class AddDraftApprenticeshipRequestValidatorTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("Fred Flintstone", true)]
        public void Validate_UserId_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { UserId = value };

            AssertValidationResult(r => r.UserId, request, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { ProviderId = value };

            AssertValidationResult(r => r.ProviderId, request, expectedValid);
        }


        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_ReservationId_ShouldBeValidated(bool hasValue, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { ReservationId = hasValue ? Guid.NewGuid() : (Guid?)null };

            AssertValidationResult(r => r.ReservationId, request, expectedValid);
        }

        [TestCase(
            "XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_FirstName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { FirstName = value };

            AssertValidationResult(r => r.FirstName, request, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_LastName_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { LastName = value };

            AssertValidationResult(r => r.LastName, request, expectedValid);
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("X", true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        public void Validate_Ref_ShouldBeValidated(string value, bool expectedValid)
        {
            var request = new AddDraftApprenticeshipRequest { OriginatorReference = value };

            AssertValidationResult(r => r.OriginatorReference, request, expectedValid);
        }

        [Test]
        public void Validate_UserInfoIsNull_ShouldBeValid()
        {
            var request = new AddDraftApprenticeshipRequest { UserInfo = null };

            AssertValidationResult(r => r.UserInfo, request, true);
        }

        [Test]
        public void Validate_UserInfoIsNotNullAndHasGoodData_ShouldBeValid()
        {
            // Arrange
            var request = new AddDraftApprenticeshipRequest { UserInfo = new UserInfo { UserId = "EE", UserDisplayName = "Name", UserEmail = "a@a.com" } };
            var validator = new AddDraftApprenticeshipRequestValidator();

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(r => r.UserInfo);
        }

        [Test]
        public void Validate_IsOnFlexiPaymentPilotIsNull_ShouldBeInvalid()
        {
            var request = new AddDraftApprenticeshipRequest { IsOnFlexiPaymentPilot = null };

            AssertValidationResult(r => r.IsOnFlexiPaymentPilot, request, false);
        }

        private static void AssertValidationResult<T>(Expression<Func<AddDraftApprenticeshipRequest, T>> property,
            AddDraftApprenticeshipRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new AddDraftApprenticeshipRequestValidator();

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