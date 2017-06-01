using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.NLog.Logger;

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
            SetUpAccountClient(5, new List<TeamMemberViewModel>());

            _sut = new EmailTemplatesService(_apprenticeshipRepostory.Object, _accountApiClient.Object, Mock.Of<IHashingService>(), Mock.Of<ILog>());
        }

        [Test]
        public async Task ThenNoAlertSummary()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>());
            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "User name",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = await _sut.GetEmails();
            _accountApiClient.Verify(m => m.GetAccountUsers(It.IsAny<string>()), Times.Never);
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
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });
            SetUpAccountClient(5, new List<TeamMemberViewModel>());

            var emails = await _sut.GetEmails();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Count().Should().Be(0);
        }

        [Test]
        public async Task Then1User1Org()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });
            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Count().Should().Be(1);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("3 apprentices");
            emails[0].Tokens["legal_entity_name"].Should().Be("Super org");
            emails[0].Tokens["changes_for_review"].Should().Be("1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("2 with requested changes");

            emails[0].RecipientsAddress.Should().Be("user@email.com");
            emails[0].TemplateId.Should().Be("EmployerAlertSummaryNotification");
            emails[0].ReplyToAddress.Should().Be("digital.apprenticeship.service@notifications.service.gov.uk");
            emails[0].Subject.Should().Be("Items for your attention: apprenticeship service");
        }

        [Test]
        public async Task Then2User1Org()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     }
                                             });
            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 1",
                                               Email = "user1@email.com",
                                               Role = "Owner"
                                           },
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 2",
                                               Email = "user2@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Length.Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user 1");
            emails[1].Tokens["name"].Should().Be("Test user 2");
            emails[0].Tokens["total_count_text"].Should().Be("3 apprentices");
            emails[0].Tokens["legal_entity_name"].Should().Be("Super org");
            emails[0].Tokens["changes_for_review"].Should().Be("1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("2 with requested changes");
        }

        [Test]
        public async Task Then1User2Org()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "999111",
                                                         LegalEntityName = "Tiny org",
                                                         ChangeOfCircCount = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });

            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Count().Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("3 apprentices");
            emails[0].Tokens["legal_entity_name"].Should().Be("Super org");
            emails[0].Tokens["changes_for_review"].Should().Be("1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("2 with requested changes");

            emails[1].Tokens["name"].Should().Be("Test user");
            emails[1].Tokens["total_count_text"].Should().Be("5 apprentices");
            emails[1].Tokens["legal_entity_name"].Should().Be("Tiny org");
            emails[1].Tokens["changes_for_review"].Should().Be("2 with changes for review");
            emails[1].Tokens["requested_changes"].Should().Be("3 with requested changes");
        }


        [Test]
        public async Task Then1User2Org2Accounts()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 6L,
                                                         LegalEntityId = "999111",
                                                         LegalEntityName = "Tiny org",
                                                         ChangeOfCircCount = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });

            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });
            SetUpAccountClient(6, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Length.Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("3 apprentices");
            emails[0].Tokens["legal_entity_name"].Should().Be("Super org");
            emails[0].Tokens["changes_for_review"].Should().Be("1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("2 with requested changes");

            emails[1].Tokens["name"].Should().Be("Test user");
            emails[1].Tokens["total_count_text"].Should().Be("5 apprentices");
            emails[1].Tokens["legal_entity_name"].Should().Be("Tiny org");
            emails[1].Tokens["changes_for_review"].Should().Be("2 with changes for review");
            emails[1].Tokens["requested_changes"].Should().Be("3 with requested changes");
        }

        [Test]
        public async Task Then2User2Org()
        {
            SetUpApprenticeshipRepostory(new List<AlertSummary>
                                             {
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 5L,
                                                         LegalEntityId = "123456",
                                                         LegalEntityName = "Super org",
                                                         ChangeOfCircCount = 1,
                                                         RestartRequestCount = 2,
                                                         TotalCount = 3
                                                     },
                                                 new AlertSummary
                                                     {
                                                         EmployerAccountId = 6L,
                                                         LegalEntityId = "999111",
                                                         LegalEntityName = "Tiny org",
                                                         ChangeOfCircCount = 2,
                                                         RestartRequestCount = 3,
                                                         TotalCount = 5
                                                     }
                                             });

            SetUpAccountClient(5, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user",
                                               Email = "user@email.com",
                                               Role = "Owner"
                                           }
                                   });
            SetUpAccountClient(6, new List<TeamMemberViewModel>
                                   {
                                       new TeamMemberViewModel
                                           {
                                               Name = "Test user 2",
                                               Email = "user2@email.com",
                                               Role = "Owner"
                                           }
                                   });

            var emails = (await _sut.GetEmails()).ToArray();
            _accountApiClient.Verify(m => m.GetAccountUsers("5"), Times.Once);
            emails.Length.Should().Be(2);
            emails[0].Tokens["name"].Should().Be("Test user");
            emails[0].Tokens["total_count_text"].Should().Be("3 apprentices");
            emails[0].Tokens["legal_entity_name"].Should().Be("Super org");
            emails[0].Tokens["changes_for_review"].Should().Be("1 with changes for review");
            emails[0].Tokens["requested_changes"].Should().Be("2 with requested changes");

            emails[1].Tokens["name"].Should().Be("Test user 2");
            emails[1].Tokens["total_count_text"].Should().Be("5 apprentices");
            emails[1].Tokens["legal_entity_name"].Should().Be("Tiny org");
            emails[1].Tokens["changes_for_review"].Should().Be("2 with changes for review");
            emails[1].Tokens["requested_changes"].Should().Be("3 with requested changes");
        }

        private void SetUpAccountClient(int accountId, List<TeamMemberViewModel> accountDetailViewModels)
        {
            _accountApiClient.Setup(m => m.GetAccountUsers(accountId.ToString()))
                .ReturnsAsync(accountDetailViewModels);
        }

        private void SetUpApprenticeshipRepostory(List<AlertSummary> alerts)
        {
            _apprenticeshipRepostory.Setup(m => m.GetEmployerApprenticeshipAlertSummary())
                .ReturnsAsync(alerts);
        }
    }
}
