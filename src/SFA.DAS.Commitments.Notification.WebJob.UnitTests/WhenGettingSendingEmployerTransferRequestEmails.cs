using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
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
        private List<TeamMemberViewModel> _accountTeamMembers;

        [SetUp]
        public void Arrange()
        {
            _apprenticeshipRepository = new Mock<ICommitmentRepository>();
            _accountApiClient = new Mock<IAccountApiClient>();

            _pendingTransferRequests = new List<TransferRequestSummary>
            {
                new TransferRequestSummary { SendingEmployerAccountId = 1, CohortReference = "TESTREF", ReceivingLegalEntityName = "Test Receiver" }
            };

            _accountDetail = new AccountDetailViewModel
            {
                HashedAccountId = "HashedSenderAccountId",
                AccountId = 1
            };

            _accountTeamMembers = new List<TeamMemberViewModel>
            {
                new TeamMemberViewModel { CanReceiveNotifications = true, Email = "user1@test.com", Role = "Owner"}
            };

            _apprenticeshipRepository.Setup(x => x.GetPendingTransferRequests())
                .ReturnsAsync(_pendingTransferRequests);

            _accountApiClient.Setup(x => x.GetAccount(It.IsAny<long>()))
                .ReturnsAsync(_accountDetail);

            _accountApiClient.Setup(x => x.GetAccountUsers(It.IsAny<long>()))
                .ReturnsAsync(_accountTeamMembers);

            _service = new SendingEmployerTransferRequestEmailService(_apprenticeshipRepository.Object,
                _accountApiClient.Object,
                Mock.Of<ILog>());
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToGetPendingTransferRequests()
        {
            await _service.GetEmails();
            _apprenticeshipRepository.Verify(x => x.GetPendingTransferRequests(), Times.Once);
        }

        [Test]
        public async Task ThenTheEmployerAccountApiIsCalledToGetAccountInfoForEachDistinctSendingEmployer()
        {
            //Arrange
            _pendingTransferRequests.Clear();
            _pendingTransferRequests.AddRange(new List<TransferRequestSummary>
            {
                new TransferRequestSummary {SendingEmployerAccountId = 1},
                new TransferRequestSummary {SendingEmployerAccountId = 2},
                new TransferRequestSummary {SendingEmployerAccountId = 2}
            });

            //Act
            await _service.GetEmails();

            //Assert
            _accountApiClient.Verify(x => x.GetAccount(It.IsAny<long>()), Times.Exactly(2));
            _accountApiClient.Verify(x => x.GetAccount(It.Is<long>(l => l == 1)), Times.Once);
            _accountApiClient.Verify(x => x.GetAccount(It.Is<long>(l => l == 2)), Times.Once);
        }

        [Test]
        public async Task ThenAnEmailIsReturnedForEachPendingTransferRequest()
        {
            //Act
            var result = await _service.GetEmails();

            //Assert
            Assert.AreEqual(_pendingTransferRequests.Count, result.Count());
        }

        [Test]
        public async Task ThenAnEmailIsReturnedForEachUserWithinAnAccount()
        {
            //Arrange
            _accountTeamMembers.Clear();
            _accountTeamMembers.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "user1@test.com", Role="Owner" });
            _accountTeamMembers.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "user2@test.com", Role = "Owner" });
            _accountTeamMembers.Add(new TeamMemberViewModel { CanReceiveNotifications = true, Email = "user3@test.com", Role = "Owner" });

            //Act
            var result = await _service.GetEmails();

            //Assert
            Assert.AreEqual(_accountTeamMembers.Count, result.Count());
        }

        [Test]
        public async Task ThenEmailsAreNotReturnedForUsersOptingOutOfNotifications()
        {
            //Arrange
            _accountTeamMembers[0].CanReceiveNotifications = false;

            //Act
            var result = await _service.GetEmails();

            //Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task ThenEmailMessageTemplateIdIsSetCorrectly()
        {
            //Act
            var result = await _service.GetEmails();

            //Assert
            Assert.AreEqual("SendingEmployerTransferRequestNotification", result.First().TemplateId);
        }

        [Test]
        public async Task ThenEmailMessageIsTokenisedCorrectly()
        {
            //Act
            var result = await _service.GetEmails();

            //Assert
            var expectedTokens = new Dictionary<string, string>
            {
                {"cohort_reference", _pendingTransferRequests[0].CohortReference},
                {"receiver_name", _pendingTransferRequests[0].ReceivingLegalEntityName},
                {"transfers_dashboard_url", $"accounts/{_accountDetail.HashedAccountId}/transfers" }
            };

            CollectionAssert.AreEquivalent(expectedTokens, result.First().Tokens);
        }
    }
}
