using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateCommitment
{
    [TestFixture]
    public sealed class WhenCreatingCommitment
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private CreateCommitmentCommandHandler _handler;
        private CreateCommitmentCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new CreateCommitmentCommandHandler(_mockCommitmentRespository.Object, new CreateCommitmentValidator());

            Fixture fixture = new Fixture();
            var populatedCommitment = fixture.Build<Commitment>().Create();
            _exampleValidRequest = new CreateCommitmentCommand { Commitment = populatedCommitment };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.Create(It.IsAny<Domain.Commitment>()));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Commitment.Name = null; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
