using NUnit.Framework;
using Ploeh.AutoFixture;
using FluentAssertions;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Application.Commands;
using Moq;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeship
{
    [TestFixture]
    public class WhenValidatingCommand
    {
        private UpdateApprenticeshipValidator _validator;
        private UpdateApprenticeshipCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();

            _validator = new UpdateApprenticeshipValidator(new ApprenticeshipValidator(new StubCurrentDateTime(), Mock.Of<IUlnValidator>(), Mock.Of<IAcademicYearValidator>()));

            var populatedCommitment = fixture.Build<Domain.Entities.Apprenticeship>().Create();
            _exampleCommand = new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 1L
                },
                CommitmentId = 123L,
                ApprenticeshipId = 333L,
                Apprenticeship = populatedCommitment
            };
        }
        
        [Test]
        public void ThenIsInvalidIfApprenticeshipIsNull()
        {
            _exampleCommand.Apprenticeship = null;
            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenCommitmentIdIsLessThanOneIsInvalid(long commitmentId)
        {
            _exampleCommand.CommitmentId = commitmentId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenApprenticeshipIdIsLessThanOneIsInvalid(long apprenticeshipId)
        {
            _exampleCommand.ApprenticeshipId = apprenticeshipId;

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void ThenProviderIdIsLessThanOneIsInvalid(long providerId)
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = providerId
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ThenIfTheAccountIdIsZeroOrLessIsNotValid(long testAccountId)
        {
            _exampleCommand.Caller = new Caller
            {
                CallerType = CallerType.Employer,
                Id = testAccountId
            };

            var result = _validator.Validate(_exampleCommand);

            result.IsValid.Should().BeFalse();
        }
    }
}
