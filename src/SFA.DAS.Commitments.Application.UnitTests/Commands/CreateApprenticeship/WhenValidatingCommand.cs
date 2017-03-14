using System;
using NUnit.Framework;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands;

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
            _validator = new CreateApprenticeshipValidator(new ApprenticeshipValidator(new StubCurrentDateTime()));
            var exampleValidApprenticeship = new Apprenticeship.Apprenticeship
            {
                FirstName = "Bob", LastName = "Smith", NINumber = ApprenticeshipTestDataHelper.CreateValidNino(),
                ULN = ApprenticeshipTestDataHelper.CreateValidULN(),
                ProviderRef = "Provider ref", EmployerRef = null,
                StartDate = DateTime.Now.AddYears(5),
                EndDate = DateTime.Now.AddYears(7)
            };

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
        public void ShouldValidateApprenticeship()
        {
            _exampleCommand.Apprenticeship = new Apprenticeship.Apprenticeship(); // Empty apprenticeship has invalid fields

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
