using FluentValidation;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.PauseApprenticeship
{
    [TestFixture]
    public class WhenPausingAnApprenticeship
    {
        PauseApprenticeshipCommandHandler _pauseApprenticeshipHandler;

        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private ApprenticeshipStatusChangeCommandValidator _validator;
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<IApprenticeshipEvents> _apprenticeshipEvents;
        private Mock<ICommitmentsLogger> _commitmentsLogger;
        private Mock<IHistoryRepository> _historyRepository;
        private Mock<IV2EventsPublisher> _v2EventsPublisher;

        private Commitment TestCommitment;
        private Apprenticeship TestApprenticeship;

        [SetUp]
        public void SetUp()
        {
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _validator = new ApprenticeshipStatusChangeCommandValidator();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _apprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _commitmentsLogger = new Mock<ICommitmentsLogger>();
            _historyRepository = new Mock<IHistoryRepository>();
            _v2EventsPublisher = new Mock<IV2EventsPublisher>();

            _pauseApprenticeshipHandler = new PauseApprenticeshipCommandHandler(
                _commitmentRepository.Object,
                _apprenticeshipRepository.Object,
                _validator,
                _currentDateTime.Object,
                _apprenticeshipEvents.Object,
                _commitmentsLogger.Object,
                _historyRepository.Object,
                _v2EventsPublisher.Object
                );

            TestCommitment = new Commitment
            {
                EmployerAccountId = 1
            };

            TestApprenticeship = new Apprenticeship
            {
                PaymentStatus = PaymentStatus.Active
            };

            _currentDateTime.Setup(t => t.Now).Returns(DateTime.Now);
        }

        [Test]
        public async Task ThenPauseApprenticeshipIsCalled()
        {
            _commitmentRepository.Setup(c => c.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(TestCommitment);

            _apprenticeshipRepository.Setup(a => a.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(TestApprenticeship);
            _apprenticeshipRepository.Setup(a => a.PauseApprenticeship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>()));

            await _pauseApprenticeshipHandler.Handle(new PauseApprenticeshipCommand { AccountId = 1, ApprenticeshipId = 2 });

            _apprenticeshipRepository.Verify(a => a.PauseApprenticeship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task ThenPauseApprenticeshipEventIsPublished()
        {
            _commitmentRepository.Setup(c => c.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(TestCommitment);

            _apprenticeshipRepository.Setup(a => a.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(TestApprenticeship);
            _apprenticeshipRepository.Setup(a => a.PauseApprenticeship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>()));

            await _pauseApprenticeshipHandler.Handle(new PauseApprenticeshipCommand { AccountId = 1, ApprenticeshipId = 2 });

            _v2EventsPublisher.Verify(e => e.PublishApprenticeshipPaused(TestCommitment, TestApprenticeship), Times.Once);
        }
    }
}
