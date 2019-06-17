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
