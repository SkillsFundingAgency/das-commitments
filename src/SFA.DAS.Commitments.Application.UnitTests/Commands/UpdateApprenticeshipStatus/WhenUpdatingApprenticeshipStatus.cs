using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    [TestFixture]
    public sealed class WhenUpdatingApprenticeshipStatus
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private UpdateApprenticeshipStatusCommandHandler _handler;
        private UpdateApprenticeshipStatusCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _exampleValidRequest = new UpdateApprenticeshipStatusCommand { AccountId = 111L, CommitmentId = 123L, ApprenticeshipId = 444L, Status = Api.Types.ApprenticeshipStatus.Approved };

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(new Apprenticeship { Status = ApprenticeshipStatus.ReadyForApproval });
            _mockCommitmentRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ApprenticeshipStatus>())).Returns(Task.FromResult(new object()));

            _handler = new UpdateApprenticeshipStatusCommandHandler(_mockCommitmentRespository.Object, new UpdateApprenticeshipStatusValidator());
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateApprenticeshipStatus(
                It.Is<long>(a => a == _exampleValidRequest.CommitmentId),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<ApprenticeshipStatus>(a => a == (ApprenticeshipStatus)_exampleValidRequest.Status)));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidRequestException>();
        }

        [Test]
        public void ThenWhenApprenticeshipIsNotInReadyForApprovalStateRequestExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(new Apprenticeship { Status = ApprenticeshipStatus.Created }) ;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
