using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingUln : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ThenULNThatIsNumericAnd10DigitsInLengthIsValid()
        {
            ExampleValidApprenticeship.ULN = "1001234567";

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeTrue();
        }

        [TestCase("abc123")]
        [TestCase("123456789")]
        [TestCase(" ")]
        [TestCase("12345678900")]
        [TestCase("0123456789")]
        public void ThenULNThatIsNotNumericAnd10DigitsInLengthIsInvalid(string uln)
        {
            ExampleValidApprenticeship.ULN = uln;

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }

        public void ThenULNThatStartsWithAZeroIsInvalid()
        {
            ExampleValidApprenticeship.ULN = "1023456789";

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }
    }
}
