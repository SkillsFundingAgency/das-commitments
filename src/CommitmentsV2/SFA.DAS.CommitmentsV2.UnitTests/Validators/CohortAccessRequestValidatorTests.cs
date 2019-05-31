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
    public class CohortAccessRequestValidatorTests
    {
        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_CohortId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.CohortId, value, expectedValid);
        }

        [TestCase(PartyType.None, false)]
        [TestCase(PartyType.TransferSender, false)]
        [TestCase(PartyType.Provider, true)]
        [TestCase(PartyType.Employer, true)]
        public void Validate_PartyType_ShouldBeValidated(PartyType value, bool expectedValid)
        {
            AssertValidationResult(request => request.PartyType, value, expectedValid);
        }

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_PartyId_ShouldBeValidated(long value, bool expectedValid)
        {
            AssertValidationResult(request => request.PartyId, value, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<CohortAccessRequest, T>> property, T value, bool expectedValid)
        {
            // Arrange
            var validator = new CohortAccessRequestValidator();

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

        private void AssertValidationResult<T>(Expression<Func<CohortAccessRequest, T>> property, Func<string, bool> feature, T value, bool expectedValid)
        {
            // Arrange
            var validator = new CohortAccessRequestValidator();

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
