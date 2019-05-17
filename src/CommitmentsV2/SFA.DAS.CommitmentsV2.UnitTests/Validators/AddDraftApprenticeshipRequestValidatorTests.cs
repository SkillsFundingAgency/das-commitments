using System;
using System.Globalization;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Validators;

namespace SFA.DAS.CommitmentsV2.UnitTests.Validators
{
    [TestFixture]
    public class AddDraftApprenticeshipRequestValidatorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_CohortId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.CohortId, value, expectedValid);
        }
        
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

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void Validate_ReservationId_ShouldBeValidated(bool useBlankGuid, bool expectedValid)
        {
            var guidToUse = useBlankGuid ? Guid.Empty : Guid.NewGuid();

            AssertValidationResult(request => request.ReservationId, guidToUse, expectedValid);
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

        private void AssertValidationResult<T>(Expression<Func<AddDraftApprenticeshipRequest,T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new AddDraftApprenticeshipRequestValidator();

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

        private void AssertValidationResult<T>(Expression<Func<AddDraftApprenticeshipRequest, T>> property, AddDraftApprenticeshipRequest instance, bool expectedValid)
        {
            // Arrange
            var validator = new AddDraftApprenticeshipRequestValidator();

            // Act
            if (expectedValid)
            {
                validator.ShouldNotHaveValidationErrorFor(property, instance);
            }
            else
            {
                validator.ShouldHaveValidationErrorFor(property, instance);
            }
        }
    }
}