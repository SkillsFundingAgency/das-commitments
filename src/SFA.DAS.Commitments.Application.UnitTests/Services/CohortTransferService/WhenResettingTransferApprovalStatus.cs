using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.CohortTransferService
{
    [TestFixture()]
    public class WhenResettingTransferApprovalStatus
    {
        private Application.Services.CohortTransferService _cohortTransferService;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IHistoryRepository> _historyRepository;
        private Mock<IApprenticeshipEventsList> _apprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _apprenticeshipEventsPublisher;
        private Commitment _commitment;

        [SetUp]
        public void Arrange()
        {
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x => x.ResetTransferApprovalStatus(It.IsAny<long>()))
                .Returns(() => Task.FromResult(new Unit()));

            _historyRepository = new Mock<IHistoryRepository>();
            _historyRepository.Setup(x => x.InsertHistory(It.IsAny<IEnumerable<HistoryItem>>()))
                .Returns(() => Task.FromResult(new Unit()));

            _apprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _apprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();

            _commitment = new Commitment
            {
                Id = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship{ Id = 999 },
                },
                TransferApprovalStatus = TransferApprovalStatus.TransferRejected
            };

            _cohortTransferService = new Application.Services.CohortTransferService(_commitmentRepository.Object,
                _historyRepository.Object, _apprenticeshipEventsList.Object, _apprenticeshipEventsPublisher.Object,
                Mock.Of<ICurrentDateTime>());
        }

        [Test]
        public async Task ThenIfTheCohortINotCurrentlyRejectedThenNoActionIsPerformed()
        {
            _commitment.TransferApprovalStatus = TransferApprovalStatus.Pending;

            await _cohortTransferService.ResetCommitmentTransferRejection(_commitment, "UserId", "UserName");

            _commitmentRepository.Verify(x => x.ResetTransferApprovalStatus(It.IsAny<long>()), Times.Never);
            _historyRepository.Verify(x => x.InsertHistory(It.IsAny<IEnumerable<HistoryItem>>()), Times.Never);
            _apprenticeshipEventsPublisher.Verify(x => x.Publish(It.IsAny<IApprenticeshipEventsList>()), Times.Never);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalled()
        {
            await _cohortTransferService.ResetCommitmentTransferRejection(_commitment, "UserId", "UserName");

            _commitmentRepository.Verify(x => x.ResetTransferApprovalStatus(It.Is<long>(id => id ==_commitment.Id)), Times.Once);
        }

        [Test]
        public async Task ThenApprenticeHistoryIsWritten()
        {
            await _cohortTransferService.ResetCommitmentTransferRejection(_commitment, "UserId", "UserName");

            _historyRepository.Verify(x => x.InsertHistory(It.Is<IEnumerable<HistoryItem>>(
                history => history.First().CommitmentId == _commitment.Id)), Times.Once);
        }

        [Test]
        public async Task ThenEventsAreWritten()
        {
            await _cohortTransferService.ResetCommitmentTransferRejection(_commitment, "UserId", "UserName");

            _apprenticeshipEventsPublisher.Verify(x => x.Publish(It.Is<IApprenticeshipEventsList>(l => l == _apprenticeshipEventsList.Object)), Times.Once);

            _apprenticeshipEventsList.Verify(x => x.Add(
                It.Is<Commitment>(c => c == _commitment),
                It.Is<Apprenticeship>(a => a == _commitment.Apprenticeships.First()),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()
                ));
        }
    }
}
