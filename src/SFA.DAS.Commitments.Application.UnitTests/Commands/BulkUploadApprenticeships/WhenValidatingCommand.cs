using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenValidatingCommand
    {
        private BulkUploadApprenticeshipsValidator _validator;
        private BulkUploadApprenticeshipsCommand _exampleCommand;
        private Mock<IUlnValidator> _mockUlnValidator;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();
            var mockApprenticeshipValidator = new Mock<AbstractValidator<Apprenticeship>>();

            _mockUlnValidator = new Mock<IUlnValidator>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();

            _validator = new BulkUploadApprenticeshipsValidator(new ApprenticeshipValidator(new StubCurrentDateTime(), _mockUlnValidator.Object, _mockAcademicYearValidator.Object));

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
            var ulnNumber = "abc123";

            _exampleCommand.Apprenticeships.ToList()[1].ULN = ulnNumber;

            _mockUlnValidator
              .Setup(m => m.Validate(ulnNumber))
              .Returns(UlnValidationResult.IsInValidTenDigitUlnNumber);

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ThenOneOfTheCostsIsInValid()
        {
            _exampleCommand.Apprenticeships.ToList()[0].Cost = 123.1232M;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutFirstNameIsNotSet(string firstName)
        {
            _exampleCommand.Apprenticeships.ToList()[1].FirstName = firstName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(null)]
        public void ThenApprenticeshipWithoutLastNameIsNotSet(string lastName)
        {
            _exampleCommand.Apprenticeships.ToList()[1].LastName = lastName;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
