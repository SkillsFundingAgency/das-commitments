using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Api.Types;
using Ploeh.AutoFixture;
using FluentAssertions;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenValidatingApprenticeships
    {
        private CreateCommitmentValidator _validator;
        private CreateCommitmentCommand _exampleCommand;

        [SetUp]
        public void Setup()
        {
            Fixture fixture = new Fixture();

            _validator = new CreateCommitmentValidator();
            var populatedCommitment = fixture.Build<Commitment>().Create();
            _exampleCommand = new CreateCommitmentCommand { Commitment = populatedCommitment };
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
    }
}
