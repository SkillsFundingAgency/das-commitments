using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SetPaymentOrder
{
    [TestFixture]
    public class WhenUpdatingPaymentOrder
    {
        private SetPaymentOrderCommandHandler _handler;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<IApprenticeshipEvents> _apprenticeshipEvents;
        private Mock<ICommitmentsLogger> _commitmentsLogger;

        [SetUp]
        public void Setup()
        {
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _commitmentsLogger = new Mock<ICommitmentsLogger>();
            _handler = new SetPaymentOrderCommandHandler(_commitmentRepository.Object, _apprenticeshipRepository.Object, _apprenticeshipEvents.Object, _commitmentsLogger.Object);
        }

        [Test]
        public async Task ThenThePaymentOrderIsUpdatedForTheAccount()
        {
            var command = new SetPaymentOrderCommand { AccountId = 123 };
            _apprenticeshipRepository.Setup(x => x.GetApprenticeshipsByEmployer(command.AccountId)).ReturnsAsync(new List<Apprenticeship>());

            await _handler.Handle(command);

            _commitmentRepository.Verify(x => x.SetPaymentOrder(command.AccountId), Times.Once);
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsUpdatedAnEventIsPublished()
        {
            var command = new SetPaymentOrderCommand { AccountId = 123 };

            var existingApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 2, CommitmentId = 3245 };
            var updatedApprenticeship = new Apprenticeship { Id = 123, PaymentOrder = 5, CommitmentId = 3245 };
            _apprenticeshipRepository.SetupSequence(x => x.GetApprenticeshipsByEmployer(command.AccountId))
                .ReturnsAsync(new List<Apprenticeship> { existingApprenticeship })
                .ReturnsAsync(new List<Apprenticeship> { updatedApprenticeship });
            
            var commitment = new Commitment();
            _commitmentRepository.Setup(x => x.GetCommitmentById(updatedApprenticeship.CommitmentId)).ReturnsAsync(commitment);

            await _handler.Handle(command);

            _apprenticeshipEvents.Verify(x => x.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-UPDATED", null, null), Times.Once);
        }
    }
}
