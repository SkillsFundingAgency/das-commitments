using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenGettingEmailTemplate
    {
        private EmailTemplatesService _sut;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepostory;
        private Mock<IAccountApiClient> _accountApiClient;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipRepostory = new Mock<IApprenticeshipRepository>();
            _accountApiClient = new Mock<IAccountApiClient>();
            SetUpApprenticeshipRepostory(new List<AlertSummary>());
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>());

            _sut = new EmailTemplatesService(
                _apprenticeshipRepostory.Object,
                _accountApiClient.Object,
                Mock.Of<ILog>());
        }

        [Test]
        public async Task ThenNoAlertSummary()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>());
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "User name",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = await _sut.GetEmails();
            _accountApiClient.Verify(m => m.GetAccountUsers(It.IsAny<long>()), Times.Never);
            emails.Count().Should().Be(0);
        }

        [Test]
        public async Task ThenAccountUsersFoundAlertSummary()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });

            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>());

            var emails = await _sut.GetEmails();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Count().Should().Be(0);
        }

        [Test]
        public async Task Then1UserWith1AccountWithAlerts()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();

            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Count().Should().Be(1);

            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("are 3 apprentices");
            emails[0].Tokens["account_name"].Should().Be("Account A");
            emails[0].Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("* 2 with requested changes");

            emails[0].RecipientsAddress.Should().Be("user@email.com");
            emails[0].TemplateId.Should().Be("EmployerAlertSummaryNotification");
            emails[0].ReplyToAddress.Should().Be("digital.apprenticeship.service@notifications.service.gov.uk");
            emails[0].Subject.Should().Be("Items for your attention: apprenticeship service");
        }

        [Test]
        public async Task WhenAccountServiceThrowingAnException()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 0,
                                                         TotalCount = 1
                                                     }
                                             });

            _accountApiClient.Setup(m => m.GetAccountUsers(5L)).Throws<Exception>();

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Exactly(4));
            emails.Length.Should().Be(0);
        }

        [Test]
        public async Task ThenShouldCreateLinkBackToFilteredManageApprenticeshipPage()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 0,
                                                         TotalCount = 1
                                                     }
                                             });
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Length.Should().Be(1);
            emails[0].Tokens["link_to_mange_apprenticeships"].Should().Be("accounts/ABC5/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=ChangeRequested");
        }

        [Test]
        public async Task Then1UserWith1AccountOneTotalCount()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 0,
                                                         TotalCount = 1
                                                     }
                                             });
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();

            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Length.Should().Be(1);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("is 1 apprentice");
            emails[0].Tokens["account_name"].Should().Be("Account A");
            emails[0].Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("");

            emails[0].RecipientsAddress.Should().Be("user@email.com");
            emails[0].TemplateId.Should().Be("EmployerAlertSummaryNotification");
            emails[0].ReplyToAddress.Should().Be("digital.apprenticeship.service@notifications.service.gov.uk");
            emails[0].Subject.Should().Be("Items for your attention: apprenticeship service");
        }

        [Test]
        public async Task Then2UsersInTheSameAccountWithAlerts()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 1",
                                               Email = "user1@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           },
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 2",
                                               Email = "user2@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Length.Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user 1");
            emails[1].Tokens["name"].Should().Be("Test user 2");
            emails[0].Tokens["total_count_text"].Should().Be("are 3 apprentices");
            emails[0].Tokens["account_name"].Should().Be("Account A");
            emails[0].Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("* 2 with requested changes");
        }

        [Test]
        public async Task Then1UserIn2AccountsWithChanges()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 10L,
                                                         ChangesForReview = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });

            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            SetUpAccountClient(10, "Account B", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Count().Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("are 3 apprentices");
            emails[0].Tokens["account_name"].Should().Be("Account A");
            emails[0].Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("* 2 with requested changes");

            emails[1].Tokens["name"].Should().Be("Test user");
            emails[1].Tokens["total_count_text"].Should().Be("are 5 apprentices");
            emails[1].Tokens["account_name"].Should().Be("Account B");
            emails[1].Tokens["changes_for_review"].Should().Be("* 2 with changes for review");
            emails[1].Tokens["requested_changes"].Should().Be("* 3 with requested changes");
        }

        [Test]
        public async Task Then2UserIn2DifferentAccountWithAlerts()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 6L,
                                                         ChangesForReview = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });

            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });
            SetUpAccountClient(6, "Account B", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 2",
                                               Email = "user2@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers(5L), Times.Once);
            emails.Length.Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("are 3 apprentices");
            emails[0].Tokens["account_name"].Should().Be("Account A");
            emails[0].Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("* 2 with requested changes");

            emails[1].Tokens["name"].Should().Be("Test user 2");
            emails[1].Tokens["total_count_text"].Should().Be("are 5 apprentices");
            emails[1].Tokens["account_name"].Should().Be("Account B");
            emails[1].Tokens["changes_for_review"].Should().Be("* 2 with changes for review");
            emails[1].Tokens["requested_changes"].Should().Be("* 3 with requested changes");
        }

        [Test]
        public async Task ThenOnlySendsEmailToUsersThatHaveNotificationsEnabled()
        {
            SetupRepoWithTwoAccounts();
            SetupAccountsWithATestUserForEachWithOneHavingNotificationsEnabled();

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(1);
        }

        private void SetupAccountsWithATestUserForEachWithOneHavingNotificationsEnabled()
        {
            SetUpAccountClient(5, "Account A", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = false
                                           }
                                   });
            SetUpAccountClient(6, "Account B", new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 2",
                                               Email = "user2@email.com",
                                               Role = "Owner",
                                               CanReceiveNotifications = true
                                           }
                                   });
        }

        private void SetupRepoWithTwoAccounts()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         ChangesForReview = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 6L,
                                                         ChangesForReview = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });
        }

        private void SetUpAccountClient(int accountId, string accountName, List<TeamMemberViewModel> accountDetailViewModels)
        {
            _accountApiClient.Setup(m => m.GetAccountUsers(accountId))
                .ReturnsAsync(accountDetailViewModels);

            _accountApiClient.Setup(m => m.GetAccount(accountId))
                .ReturnsAsync(new AccountDetailViewModel { AccountId = accountId, DasAccountName = accountName, HashedAccountId = $"ABC{accountId}" });
        }

        private void SetUpApprenticeshipRepostory(List<AlertSummary> alerts)
        {
            _apprenticeshipRepostory.Setup(m => m.GetEmployerApprenticeshipAlertSummary())
                .ReturnsAsync(alerts);
        }
    }
}
