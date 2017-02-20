using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingProviderReference : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ThenProviderReferenceOver20CharacterIsInvalid()
        {
            ExampleValidApprenticeship.ProviderRef = "opopopopopopopopopopa"; // 21 characters

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }
    }
}
