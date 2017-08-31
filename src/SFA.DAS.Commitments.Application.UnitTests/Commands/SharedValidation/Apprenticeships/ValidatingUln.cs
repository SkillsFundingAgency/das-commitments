using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingUln : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ShouldCallUlnValidatorService()
        {
            ExampleValidApprenticeship.ULN = "123456789";

            MockUlnValidator
                .Setup(m => m.Validate(ExampleValidApprenticeship.ULN))
                .Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);

            Validator.Validate(ExampleValidApprenticeship);

            MockUlnValidator
               .Verify(m => m.Validate(ExampleValidApprenticeship.ULN), Times.AtLeastOnce);
        }

        [Test]
        public void ShouldBeInvalidIfResultIsNotSuccess()
        {
            ExampleValidApprenticeship.ULN = "123456789";

            MockUlnValidator
                .Setup(m => m.Validate(ExampleValidApprenticeship.ULN))
                .Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldBeValidIfResultIsSuccess()
        {
            ExampleValidApprenticeship.ULN = "1748529632";

            MockUlnValidator
             .Setup(m => m.Validate(ExampleValidApprenticeship.ULN))
             .Returns(UlnValidationResult.Success);

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }
    }
}
