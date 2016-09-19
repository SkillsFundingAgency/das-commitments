﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Tasks.Api.Client;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentStatus
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentStatus
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateCommitmentStatusCommandHandler _handler;
        private UpdateCommitmentStatusCommand _exampleValidRequest;
        private Mock<ITasksApi> _tasksApi;

        [SetUp]
        public void SetUp()
        {
            _tasksApi = new Mock<ITasksApi>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockCommitmentRespository.Setup(x => x.UpdateStatus(It.IsAny<long>(), It.IsAny<CommitmentStatus>())).Returns(Task.FromResult(new object()));
            _handler = new UpdateCommitmentStatusCommandHandler(_tasksApi.Object, _mockCommitmentRespository.Object, new UpdateCommitmentStatusValidator());

            _exampleValidRequest = new UpdateCommitmentStatusCommand { AccountId = 111L, CommitmentId = 123L, Status = Api.Types.CommitmentStatus.Active };
        }

        [Test]
        public async Task ThenShouldCallTheRepository()
        {
            var commitment = new Commitment
            {
                Status = CommitmentStatus.Draft
            };

            _mockCommitmentRespository.Setup(x => x.GetById(It.IsAny<long>())).ReturnsAsync(commitment);

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateStatus(
                It.Is<long>(a => a == _exampleValidRequest.CommitmentId), 
                It.Is<CommitmentStatus>(a => a == (CommitmentStatus)_exampleValidRequest.Status)));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
