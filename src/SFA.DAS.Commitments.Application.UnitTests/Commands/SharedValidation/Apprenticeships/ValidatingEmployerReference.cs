using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    [TestFixture]
    public sealed class ValidatingEmployerReference : ApprenticeshipValidationTestBase
    {
        [Test]
        public void ThenEmployerReferenceOver20CharacterIsInvalid()
        {
            ExampleValidApprenticeship.EmployerRef = "opopopopopopopopopopa"; // 21 characters

            var result = Validator.Validate(ExampleValidApprenticeship);

            result.IsValid.Should().BeFalse();
        }
    }
}
