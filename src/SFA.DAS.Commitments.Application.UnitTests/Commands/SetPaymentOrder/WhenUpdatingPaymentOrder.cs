using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SetPaymentOrder
{
    [TestFixture]
    public class WhenUpdatingPaymentOrder
    {
        private SetPaymentOrderCommandHandler _handler;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _apprenticeshipEventsPublisher;
        private Mock<ICommitmentsLogger> _commitmentsLogger;
        private Mock<ICurrentDateTime> _currentDateTime;
        private readonly DateTime CurrentDateTime = DateTime.UtcNow;

        [SetUp]
        public void Setup()
        {
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _apprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _commitmentsLogger = new Mock<ICommitmentsLogger>();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _currentDateTime.SetupGet(x => x.Now).Returns(CurrentDateTime);

            _handler = new SetPaymentOrderCommandHandler(
                _commitmentRepository.Object, 
                _apprenticeshipRepository.Object, 
                _apprenticeshipEventsList.Object, 
                _apprenticeshipEventsPublisher.Object, 
                _commitmentsLogger.Object,
                _currentDateTime.Object);
        }

        [Test]
        public async Task ThenThePaymentOrderIsUpdatedForTheAccount()
        {
            var command = new SetPaymentOrderCommand { AccountId = 123 };
            _apprenticeshipRepository.Setup(x => x.GetApprenticeshipsByEmployer(command.AccountId, "")).ReturnsAsync(new ApprenticeshipsResult {Apprenticeships = new List<Apprenticeship>()});

            await _handler.Handle(command);

            _commitmentRepository.Verify(x => x.SetPaymentOrder(command.AccountId), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsUpdatedAnEventIsPublished()
        {
            var command = new SetPaymentOrderCommand { AccountId = 123 };

            var existingApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 2, CommitmentId = 3245 };
            var updatedApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 5, CommitmentId = 3245 };
            _apprenticeshipRepository.SetupSequence(x => x.GetApprenticeshipsByEmployer(command.AccountId, ""))
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = new List<Apprenticeship> { existingApprenticeship } })
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = new List<Apprenticeship> { updatedApprenticeship } });
            
            var commitment = new Commitment { Id = 3245 };
            _commitmentRepository.Setup(x => x.GetCommitmentById(updatedApprenticeship.CommitmentId)).ReturnsAsync(commitment);

            await _handler.Handle(command);

            _apprenticeshipEventsList.Verify(x => x.Add(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED", CurrentDateTime.Date, null), Times.Once);
            _apprenticeshipEventsPublisher.Verify(x => x.Publish(_apprenticeshipEventsList.Object), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsNotUpdatedAnEventIsNotPublished()
        {
            var command = new SetPaymentOrderCommand { AccountId = 123 };

            var existingApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 2, CommitmentId = 3245 };
            var updatedApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 2, CommitmentId = 3245 };
            _apprenticeshipRepository.SetupSequence(x => x.GetApprenticeshipsByEmployer(command.AccountId, ""))
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = new List<Apprenticeship> { existingApprenticeship } })
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = new List<Apprenticeship> { updatedApprenticeship } });

            var commitment = new Commitment { Id = 3245 };
            _commitmentRepository.Setup(x => x.GetCommitmentById(updatedApprenticeship.CommitmentId)).ReturnsAsync(commitment);

            await _handler.Handle(command);

            _apprenticeshipEventsList.Verify(x => x.Add(It.IsAny<Commitment>(),
                    It.IsAny<Apprenticeship>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime?>()),
                Times.Never);

            _apprenticeshipEventsPublisher.Verify(x => x.Publish(_apprenticeshipEventsList.Object), Times.Never);
        }
    }
}
