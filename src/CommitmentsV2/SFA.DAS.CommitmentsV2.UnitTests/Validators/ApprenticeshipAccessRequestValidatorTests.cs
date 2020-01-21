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
    public class ApprenticeshipAccessRequestValidatorTests
    {
        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_ApprenticeshipId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.ApprenticeshipId, value, expectedValid);
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.TransferSender, false)]
        [TestCase(Party.Provider, true)]
        [TestCase(Party.Employer, true)]
        public void Validate_PartyType_ShouldBeValidated(Party value, bool expectedValid)
        {
            AssertValidationResult(request => request.Party, value, expectedValid);
        }

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_PartyId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.PartyId, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<ApprenticeshipAccessRequest, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new ApprenticeshipAccessRequestValidator();

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

        private void AssertValidationResult<T>(Expression<Func<ApprenticeshipAccessRequest, T>> property, Func<string, bool> feature, T value, bool expectedValid)
        {
            // Arrange
            var validator = new ApprenticeshipAccessRequestValidator();

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
