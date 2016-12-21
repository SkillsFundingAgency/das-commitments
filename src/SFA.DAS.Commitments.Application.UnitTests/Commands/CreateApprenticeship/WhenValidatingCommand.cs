using NUnit.Framework;
using Ploeh.AutoFixture;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateApprenticeship
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private CreateApprenticeshipValidator _validator;
        private CreateApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();

            _validator = new CreateApprenticeshipValidator();
            var exampleValidApprenticeship = new Apprenticeship { FirstName = "Bob", LastName = "Smith" };
            _exampleCommand = new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                },
                CommitmentId = 123L,
                Apprenticeship = exampleValidApprenticeship
            };
        }
        
        [Test]
        public void ThenIsInvalidIfApprenticeshipIsNull()
        {
            _exampleCommand.Apprenticeship = null;
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 1
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenCommitmentIdIsLessThanOneIsInvalid(long commitmentId)
        {
            _exampleCommand.CommitmentId = commitmentId;
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 1
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenIdIsGreaterThanZeroIsValid()
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = 12
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenIfIdIsLessThanOneIsInvalid(long id)
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = id
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenULNThatIsNumericAnd10DigitsInLengthIsValid()
        {
            _exampleCommand.Apprenticeship.ULN = "0001234567";

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase("abc123")]
        [TestCase("123456789")]
        [TestCase(" ")]
        [TestCase("")]
        public void ThenULNThatIsNotNumericAnd10DigitsInLengthIsInvalid(string uln)
        {
            _exampleCommand.Apprenticeship.ULN = uln;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        public void ThenULNThatStartsWithAZeroIsInvalid()
        {
            _exampleCommand.Apprenticeship.ULN = "0123456789";

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(123.12)]
        [TestCase(123.1)]
        [TestCase(123.0)]
        [TestCase(123)]
        [TestCase(123.000)]
        public void ThenCostThatIsNumericAndHas2DecimalPlacesIsValid(decimal cost)
        {
            _exampleCommand.Apprenticeship.Cost = cost;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(123.1232)]
        [TestCase(0.001)]
        public void ThenCostThatIsNotAMax2DecimalPlacesIsInvalid(decimal cost)
        {
            _exampleCommand.Apprenticeship.Cost = cost;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-0)]
        [TestCase(-123.12)]
        [TestCase(-123)]
        [TestCase(-123.1232)]
        [TestCase(-0.001)]
        public void ThenCostThatIsZeroOrNegativeNumberIsInvalid(decimal cost)
        {
            _exampleCommand.Apprenticeship.Cost = cost;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutFirstNameIsNotSet(string firstName)
        {
            _exampleCommand.Apprenticeship.FirstName = firstName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutLastNameIsNotSet(string lastName)
        {
            _exampleCommand.Apprenticeship.LastName = lastName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
