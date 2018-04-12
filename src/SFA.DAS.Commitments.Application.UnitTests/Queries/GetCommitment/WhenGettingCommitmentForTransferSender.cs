using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using Moq;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;
using System;
using AutoFixture;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Application.Rules;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetCommitment
{

    [TestFixture]
    public class WhenGettingCommitmentForTransferSender
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private GetCommitmentQueryHandler _handler;
        private GetCommitmentRequest _exampleValidRequest;
        private Commitment _fakeCommitment;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetCommitmentQueryHandler(_mockCommitmentRespository.Object, new GetCommitmentValidator(),
                new CommitmentRules());

            var dataFixture = new Fixture();
            _fakeCommitment = dataFixture.Build<Commitment>().Create();
            _exampleValidRequest = new GetCommitmentRequest
            {
                CommitmentId = _fakeCommitment.Id,
                Caller = new Caller
                {
                    CallerType = CallerType.TransferSender,
                    Id = (long) _fakeCommitment?.TransferSenderId.Value
                }
            };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(_fakeCommitment);
        }

        [Test]
        public async Task ThenShouldReturnACommitmentIfTransferSenderMatches()
        {

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Id.Should().Be(_fakeCommitment.Id);
            response.Data.Reference.Should().Be(_fakeCommitment.Reference);
            response.Data.Apprenticeships.Should().HaveSameCount(_fakeCommitment.Apprenticeships);
            response.Data.Messages.Should().HaveSameCount(_fakeCommitment.Messages);
        }

        [Test]
        public void ThenAnExceptionIsThrownWhenTheCommitmentTransferSenderDoesntMatch()
        {
            _fakeCommitment.TransferSenderId++;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>().WithMessage(
                $"Transfer Sender {_exampleValidRequest.Caller.Id} not authorised to access commitment {_fakeCommitment.Id}, expected transfer sender {_fakeCommitment.TransferSenderId}");
        }
    }
}
