using System.Collections.Generic;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private BulkUploadApprenticeshipsValidator _validator;
        private BulkUploadApprenticeshipsCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();
            var mockApprenticeshipValidator = new Mock<AbstractValidator<Apprenticeship>>();

            _validator = new BulkUploadApprenticeshipsValidator(new ApprenticeshipValidator(new StubCurrentDateTime()));

            var exampleValidApprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { FirstName = "Bob", LastName = "Smith" },
                new Apprenticeship { FirstName = "Bill", LastName = "Jones" }
            };

            _exampleCommand = new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1
                },
                CommitmentId = 123L,
                Apprenticeships = exampleValidApprenticeships
            };
        }

        [Test]
        public void ThenIsInvalidIfApprenticeshipsIsNull()
        {
            _exampleCommand.Apprenticeships = null;
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = 1
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        public void ThenIsInvalidIfApprenticeshipsIsEmpty()
        {
            _exampleCommand.Apprenticeships = new List<Apprenticeship>();
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
        public void ThenOneOfTheULNsIsInValid()
        {
            _exampleCommand.Apprenticeships[1].ULN = "abc123";

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenOneOfTheCostsIsInValid()
        {
            _exampleCommand.Apprenticeships[0].Cost = 123.1232M;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutFirstNameIsNotSet(string firstName)
        {
            _exampleCommand.Apprenticeships[1].FirstName = firstName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutLastNameIsNotSet(string lastName)
        {
            _exampleCommand.Apprenticeships[1].LastName = lastName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
