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
            AssertValidationResult(request => request.UserId, value, expectedValid);
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_ProviderId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.ProviderId, value, expectedValid);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Validate_ReservationId_ShouldBeValidated(bool hasValue, bool expectedValid)
        {
            AssertValidationResult(request => request.ReservationId, hasValue ? Guid.NewGuid() : (Guid?)null, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_FirstName_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.FirstName, value, expectedValid);
        }

        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXX100", false)]
        [TestCase("XXXXXXXXX1XXXXXXXXX2XXXXXXXXX3XXXXXXXXX4XXXXXXXXX5XXXXXXXXX6XXXXXXXXX7XXXXXXXXX8XXXXXXXXX9XXXXXXXXXX", true)]
        [TestCase("", true)]
        public void Validate_LastName_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.LastName, value, expectedValid);
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("X", true)]
        [TestCase("XXXXXXXXX1XXXXXXXXX20", false)]
        public void Validate_Ref_ShouldBeValidated(string value, bool expectedValid)
        {
            AssertValidationResult(request => request.OriginatorReference, value, expectedValid);
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

        [Test]
        public void Validate_IsOnFlexiPaymentPilotIsNull_ShouldBeInvalid()
        {
            AssertValidationResult(request => request.IsOnFlexiPaymentPilot, (bool?)null, false);
        }

        [Test]
        public void Validate_LearnerVerificationResponseIsNull_ShouldBeInvalid()
        {
            AssertValidationResult(request => request.LearnerVerificationResponse, null, false);
        }

        private void AssertValidationResult<T>(Expression<Func<AddDraftApprenticeshipRequest, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new AddDraftApprenticeshipRequestValidator(Mock.Of<IAuthorizationService>());

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

        private void AssertValidationResult<T>(Expression<Func<AddDraftApprenticeshipRequest, T>> property, Func<string, bool> feature, T value, bool expectedValid)
        {
            // Arrange
            var authorizationService = new Mock<IAuthorizationService>();

            authorizationService.Setup(a => a.IsAuthorized(It.IsAny<string[]>()))
                .Returns<string[]>(o => feature(o.SingleOrDefault()));

            var validator = new AddDraftApprenticeshipRequestValidator(authorizationService.Object);

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