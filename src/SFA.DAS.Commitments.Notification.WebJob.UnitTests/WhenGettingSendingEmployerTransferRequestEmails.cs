using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenGettingSendingEmployerTransferRequestEmails
    {
        private SendingEmployerTransferRequestEmailService _service;
        private Mock<ICommitmentRepository> _apprenticeshipRepository;
        private Mock<IAccountApiClient> _accountApiClient;
        private List<TransferRequestSummary> _pendingTransferRequests;
        private AccountDetailViewModel _accountDetail;

        private Func<Task> _act;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<ICommitmentRepository>();
            _accountApiClient = new Mock<IAccountApiClient>();

            _pendingTransferRequests = new List<TransferRequestSummary>();
            _accountDetail = new AccountDetailViewModel();

            _apprenticeshipRepository.Setup(x => x.GetPendingTransferRequests())
                .ReturnsAsync(_pendingTransferRequests);

            _accountApiClient.Setup(x => x.GetAccount(It.IsAny<long>()))
                .ReturnsAsync(_accountDetail);

            _service = new SendingEmployerTransferRequestEmailService(_apprenticeshipRepository.Object,
                _accountApiClient.Object,
                Mock.Of<ILog>());

            _act = async () => await _service.GetEmails();
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetPendingTransferRequests()
        {
            await _act();
            _apprenticeshipRepository.Verify(x => x.GetPendingTransferRequests(), Times.Once);
        }

        [Test]
        public async Task ThenTheEmployerAccountApiIsCalledToGetAccountInfoForEachDistinctSendingEmployer()
        {
            //Arrange
            _pendingTransferRequests.AddRange(new List<TransferRequestSummary>
            {
                new TransferRequestSummary { SendingEmployerAccountId = 1 },
                new TransferRequestSummary { SendingEmployerAccountId = 2 },
                new TransferRequestSummary { SendingEmployerAccountId = 2 }
            });

            //Act
            await _act();

            //Assert
            _accountApiClient.Verify(x => x.GetAccount(It.IsAny<long>()), Times.Exactly(2));
            _accountApiClient.Verify(x => x.GetAccount(It.Is<long>(l => l == 1)), Times.Once);
            _accountApiClient.Verify(x => x.GetAccount(It.Is<long>(l => l == 2)), Times.Once);
        }

        [Test]
        public async Task ThenAnEmailIsReturnedForEachPendingTransferRequest()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task ThenAnEmailIsReturnedForEachUserWithinAnAccount()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task ThenAnEmailIsNotReturnedForUsersOptingOutOfNotifications()
        {
            throw new NotImplementedException();
        }

    }
}
