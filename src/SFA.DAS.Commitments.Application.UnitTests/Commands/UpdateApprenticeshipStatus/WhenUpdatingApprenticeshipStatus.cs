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
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private UpdateApprenticeshipStatusCommandHandler _handler;
        private UpdateApprenticeshipStatusCommand _exampleValidRequest;

        [SetUp]
        public void SetUp()
        {
            _exampleValidRequest = new UpdateApprenticeshipStatusCommand {AccountId = 111L, ApprenticeshipId = 444L, PaymentStatus = Api.Types.Apprenticeship.Types.PaymentStatus.Active};
            var _testApprenticeship = new Apprenticeship { CommitmentId = 123L, PaymentStatus = PaymentStatus.PendingApproval };

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();

            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(_testApprenticeship);
            _mockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>())).Returns(Task.FromResult(new object()));

            _handler = new UpdateApprenticeshipStatusCommandHandler(_mockCommitmentRespository.Object, _mockApprenticeshipRespository.Object, new UpdateApprenticeshipStatusValidator(), Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenShouldCallTheRepositoryToUpdateTheStatus()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(new Commitment
            {
                Id = 123L,
                EmployerAccountId = _exampleValidRequest.AccountId
            });

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.UpdateApprenticeshipStatus(
                It.Is<long>(a => a == 123L),
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
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(123L)).ReturnsAsync(new Commitment
            {
                Id = 123L,
                ProviderId = _exampleValidRequest.AccountId++
            });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [TestCase(PaymentStatus.PendingApproval, Api.Types.Apprenticeship.Types.PaymentStatus.Active)]
        [TestCase(PaymentStatus.Active, Api.Types.Apprenticeship.Types.PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Paused, Api.Types.Apprenticeship.Types.PaymentStatus.Active)]
        public void ThenWhenStateTransitionIsValidNoExceptionIsThrown(PaymentStatus initial, Api.Types.Apprenticeship.Types.PaymentStatus target)
        {
            var apprenticeshipFromRepository = new Apprenticeship {PaymentStatus = initial};
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(apprenticeshipFromRepository);
            _exampleValidRequest.PaymentStatus = target;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldNotThrow<InvalidRequestException>();
        }

        [TestCase(PaymentStatus.PendingApproval, Api.Types.Apprenticeship.Types.PaymentStatus.Paused)]
        [TestCase(PaymentStatus.Paused, Api.Types.Apprenticeship.Types.PaymentStatus.PendingApproval)]
        public void ThenWhenApprenticeshipNotInValidStateRequestThrowsException(PaymentStatus initial, Api.Types.Apprenticeship.Types.PaymentStatus target)
        {
            var apprenticeshipFromRepository = new Apprenticeship {PaymentStatus = initial};
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == _exampleValidRequest.ApprenticeshipId))).ReturnsAsync(apprenticeshipFromRepository);
            _exampleValidRequest.PaymentStatus = target;

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<Exception>();
        }
    }
}
