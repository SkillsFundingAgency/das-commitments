using System.Linq.Expressions;
using FluentValidation.TestHelper;
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
            var request = new CohortAccessRequest { CohortId = value };
            
            AssertValidationResult(r => r.CohortId, request, expectedValid);
        }

        [TestCase(Party.None, false)]
        [TestCase(Party.TransferSender, false)]
        [TestCase(Party.Provider, true)]
        [TestCase(Party.Employer, true)]
        public void Validate_PartyType_ShouldBeValidated(Party value, bool expectedValid)
        {
            var request = new CohortAccessRequest { Party = value };
            
            AssertValidationResult(r => r.Party, request, expectedValid);
        }

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        public void Validate_PartyId_ShouldBeValidated(long value, bool expectedValid)
        {
            var request = new CohortAccessRequest { PartyId = value };
            
            AssertValidationResult(r => r.PartyId, request, expectedValid);
        }

        private void AssertValidationResult<T>(Expression<Func<CohortAccessRequest, T>> property, CohortAccessRequest request, bool expectedValid)
        {
            // Arrange
            var validator = new CohortAccessRequestValidator();
            
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
