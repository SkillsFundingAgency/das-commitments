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
            var request = new ApprenticeshipAccessRequest { ApprenticeshipId = value };
            
            AssertValidationResult(r => r.ApprenticeshipId, request, expectedValid);
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.TransferSender, false)]
        [TestCase(Party.Provider, true)]
        [TestCase(Party.Employer, true)]
        public void Validate_PartyType_ShouldBeValidated(Party value, bool expectedValid)
        {
            var request = new ApprenticeshipAccessRequest { Party = value };
            
            AssertValidationResult(r => r.Party, request, expectedValid);
        }

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_PartyId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new ApprenticeshipAccessRequest { PartyId = value };
            
            AssertValidationResult(r => r.PartyId, request, expectedValid);
        }

        private static void AssertValidationResult<T>(Expression<Func<ApprenticeshipAccessRequest, T>> property, ApprenticeshipAccessRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new ApprenticeshipAccessRequestValidator();

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
