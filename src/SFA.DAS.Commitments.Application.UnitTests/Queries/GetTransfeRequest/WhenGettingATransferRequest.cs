using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequest;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetTransfeRequest
{
    [TestFixture]
    public class WhenGettingATransferRequest
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;

        private GetTransferRequestQueryHandler _handler;
        private GetTransferRequestRequest _exampleValidRequest;
        private TransferRequest _fakeTransferRequest;

        [SetUp]
        public void SetUp()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _handler = new GetTransferRequestQueryHandler(_mockCommitmentRespository.Object, new GetTransferRequestValidator(), new CommitmentRules());

            Fixture dataFixture = new Fixture();
            _fakeTransferRequest = dataFixture.Build<TransferRequest>().Create();
            _exampleValidRequest = new GetTransferRequestRequest
            {
                TransferRequestId = _fakeTransferRequest.TransferRequestId,
                Caller = new Caller
                {
                    CallerType = CallerType.TransferSender,
                    Id = _fakeTransferRequest.SendingEmployerAccountId
                }
            };
            _mockCommitmentRespository.Setup(x => x.GetTransferRequest(It.IsAny<long>())).ReturnsAsync(_fakeTransferRequest);
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.GetTransferRequest(_fakeTransferRequest.TransferRequestId), Times.Once);
        }

        [Test]
        public async Task ThenIfNoTransferFoundShouldReturnANullDataElementInResponse()
        {
            _mockCommitmentRespository.Setup(x => x.GetTransferRequest(It.IsAny<long>())).ReturnsAsync(null);

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Should().Be(null);
        }
       
        [Test]
        public async Task ThenShouldReturnACommitmentInResponse()
        {
            _mockCommitmentRespository.Setup(x => x.GetTransferRequest(It.IsAny<long>())).ReturnsAsync(_fakeTransferRequest);

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Should().Be(_fakeTransferRequest);
        }

        [Test]
        public async Task ThenShouldEnsureTransferSenderIdMatches()
        {
            Func<Task> act = async () =>
            {
                _fakeTransferRequest.SendingEmployerAccountId = 100;
                _exampleValidRequest.Caller.Id = 200;
                _exampleValidRequest.Caller.CallerType = CallerType.TransferSender;

                await _handler.Handle(_exampleValidRequest);
            };
            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public async Task ThenShouldEnsureTransferReceiverIdMatches()
        {
            Func<Task> act = async () =>
            {
                _fakeTransferRequest.ReceivingEmployerAccountId = 100;
                _exampleValidRequest.Caller.Id = 200;
                _exampleValidRequest.Caller.CallerType = CallerType.TransferReceiver;

                await _handler.Handle(_exampleValidRequest);
            };
            act.ShouldThrow<UnauthorizedException>();
        }
    }
}
