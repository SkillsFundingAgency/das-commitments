using System;

using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Api.Types;
using Ploeh.AutoFixture;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenValidatingApprenticeships
    {
        private CreateCommitmentValidator _validator;
        private CreateCommitmentCommand _exampleCommand;
        private Apprenticeship _validApprenticeship;

        [SetUp]
        public void Setup()
        {
            Fixture fixture = new Fixture();

            _validator = new CreateCommitmentValidator();
            var populatedCommitment = fixture.Build<Commitment>().Create();
            _exampleCommand = new CreateCommitmentCommand { Commitment = populatedCommitment };
            
            _validApprenticeship = new Apprenticeship
                { FirstName = "First name A", LastName = "Last name B" };
        }

        [Test]
        public void ValidateEmptyNames()
        {
            _validApprenticeship.FirstName = "";
            _validApprenticeship.LastName = "";
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);

            result.Errors.Any(x => x.ErrorMessage == "'First Name' should not be empty.").Should().BeTrue();
            result.Errors.Any(x => x.ErrorMessage == "'Last Name' should not be empty.").Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ValidateTooLongNames()
        {
            _validApprenticeship.FirstName = new string('*', 100);
            _validApprenticeship.LastName = new string('*', 100);
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors.Any(x => x.ErrorMessage == "The specified condition was not met for 'First Name'.").Should().BeTrue();
            result.Errors.Any(x => x.ErrorMessage == "The specified condition was not met for 'Last Name'.").Should().BeTrue();
            result.Errors.Count.Should().Be(2);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void IfAnApprenticeshipContainsAnInvalidULNIsInvalid()
        {
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { ULN = "1234567890" },
                new Apprenticeship { ULN = "000111111" } // Invalid
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void IfAnApprenticeshipContainsAnInvalidULNIsInvalid10Times9()
        {
            _validApprenticeship.ULN = "9999999999";
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { ULN = "1234567890", FirstName = "Abba", LastName = "Sabba" },
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors.Count.Should().Be(1);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void IfAnApprenticeshipContainsAnInvalidCostIsInvalid()
        {
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { Cost = 123.45M },
                new Apprenticeship { Cost = 123.456M } // Invalid
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void IfAnApprenticeshipContainsAnInvalidCostIsInvalid2()
        {
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { Cost = 123.45M },
                new Apprenticeship { Cost = 1234567.45M } // Invalid
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("", 1)]
        [TestCase("AB12345678AA", 1)]
        [TestCase("AB1234560", 1)]
        [TestCase("AB123456", 1)]
        [TestCase("A123456", 1)]
        [TestCase("Db123456A", 1)]
        [TestCase("AB123456A", 0)]
        [TestCase("AB123456 ", 0)]
        [TestCase(null, 0)]
        public void IfAnApprenticeshipContainsAnInvalidNinoIsInvalid(string nino, int errorCount)
        {
            _validApprenticeship.NINumber = nino;
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors.Count.Should().Be(errorCount);
        }

        [TestCase("###### 21 CHARs #####")]
        public void IfAnApprenticeshipContainsAnInvalidProviderRef(string reference)
        {
            _validApprenticeship.ProviderRef = reference;
            _validApprenticeship.EmployerRef = reference;
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors[0].ErrorMessage.Should().Be("'Provider Ref' must be between 0 and 20 characters. You entered 21 characters.");
            result.Errors[1].ErrorMessage.Should().Be("'Employer Ref' must be between 0 and 20 characters. You entered 21 characters.");
            result.Errors.Count.Should().Be(2);
        }

        [TestCase("###### 20 CHARs ####")]
        [TestCase("")]
        [TestCase("Abba reference")]
        [TestCase(null)]
        public void IfAnApprenticeshipContainsAValidProviderRef(string reference)
        {
            _validApprenticeship.ProviderRef = reference;
            _validApprenticeship.EmployerRef = reference;
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors.Count.Should().Be(0);
        }

        [Test]
        public void EndDateIsInPast()
        {
            _validApprenticeship.StartDate = DateTime.Now.AddYears(-7);
            _validApprenticeship.EndDate = DateTime.Now.AddYears(-5);
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors[0].ErrorMessage.Should().Be("The specified condition was not met for 'End Date'.");
            result.Errors.Count.Should().Be(1);
        }

        [Test]
        public void EndDateAfterStartDate()
        {
            _validApprenticeship.StartDate = DateTime.Now.AddYears(5);
            _validApprenticeship.EndDate = DateTime.Now.AddYears(2);
            _exampleCommand.Commitment.Apprenticeships = new List<Apprenticeship>
            {
                _validApprenticeship
            };

            var result = _validator.Validate(_exampleCommand);
            result.Errors[0].ErrorMessage.Should().Be("The specified condition was not met for 'End Date'.");
            result.Errors.Count.Should().Be(1);
        }
    }
}
