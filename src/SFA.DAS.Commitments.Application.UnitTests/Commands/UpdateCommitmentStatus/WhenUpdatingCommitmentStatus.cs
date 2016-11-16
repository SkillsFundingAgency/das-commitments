using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Client;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentStatus
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentStatus
    {
        private Mock<IEventsApi> _mockEventsApi;
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateCommitmentStatusCommandHandler _handler;
        private UpdateCommitmentStatusCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _mockEventsApi = new Mock<IEventsApi>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockCommitmentRespository.Setup(x => x.UpdateCommitmentStatus(It.IsAny<long>(), It.IsAny<CommitmentStatus>())).Returns(Task.FromResult(new object()));
            _handler = new UpdateCommitmentStatusCommandHandler(_mockCommitmentRespository.Object, new UpdateCommitmentStatusValidator(), _mockEventsApi.Object);

            _exampleValidRequest = new UpdateCommitmentStatusCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 111L
                },
                CommitmentId = 123L,
                CommitmentStatus = Api.Types.CommitmentStatus.Active
            };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            var commitment = new Commitment
            {
                CommitmentStatus = CommitmentStatus.New,
                Id = _exampleValidRequest.CommitmentId,
                EmployerAccountId = _exampleValidRequest.Caller.Id
            };

            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(commitment);

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateCommitmentStatus(
                It.Is<long>(a => a == _exampleValidRequest.CommitmentId), 
                It.Is<CommitmentStatus>(a => a == (CommitmentStatus)_exampleValidRequest.CommitmentStatus)));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.Caller = new Caller {CallerType = CallerType.Employer, Id = 0}; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.Caller.Id++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        //todo: add unit test for events api call
    }
}
