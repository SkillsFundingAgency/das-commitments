using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

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
            _exampleValidRequest = new UpdateApprenticeshipStatusCommand {AccountId = 111L, CommitmentId = 123L, ApprenticeshipId = 444L, PaymentStatus = Api.Types.PaymentStatus.Active};

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(new Apprenticeship {PaymentStatus = PaymentStatus.PendingApproval});
            _mockCommitmentRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>())).Returns(Task.FromResult(new object()));

            _handler = new UpdateApprenticeshipStatusCommandHandler(_mockCommitmentRespository.Object, new UpdateApprenticeshipStatusValidator(), Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockCommitmentRespository.Verify(x => x.UpdateApprenticeshipStatus(
                It.Is<long>(a => a == _exampleValidRequest.CommitmentId),
                It.Is<long>(a => a == _exampleValidRequest.ApprenticeshipId),
                It.Is<PaymentStatus>(a => a == (PaymentStatus) _exampleValidRequest.PaymentStatus)));
        }

        [Test]
        public void ThenWhenValidationFailsAnInvalidRequestExceptionIsThrown()
        {
            _exampleValidRequest.AccountId = 0; // Forces validation failure

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenWhenUnauthorisedAnUnauthorizedExceptionIsThrown()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(_exampleValidRequest.CommitmentId)).ReturnsAsync(new Commitment
            {
                Id = _exampleValidRequest.CommitmentId,
                ProviderId = _exampleValidRequest.AccountId++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [TestCase(PaymentStatus.PendingApproval, Api.Types.PaymentStatus.Active)]
        [TestCase(PaymentStatus.Active, Api.Types.PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Paused, Api.Types.PaymentStatus.Active)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial, Api.Types.PaymentStatus target)
        {
            var apprenticeshipFromRepository = new Apprenticeship {PaymentStatus = initial};
            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(apprenticeshipFromRepository);
            _exampleValidRequest.PaymentStatus = target;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }

        [TestCase(PaymentStatus.PendingApproval, Api.Types.PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Paused, Api.Types.PaymentStatus.PendingApproval)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial, Api.Types.PaymentStatus target)
        {
            var apprenticeshipFromRepository = new Apprenticeship {PaymentStatus = initial};
            _mockCommitmentRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(apprenticeshipFromRepository);
            _exampleValidRequest.PaymentStatus = target;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }
    }
}
