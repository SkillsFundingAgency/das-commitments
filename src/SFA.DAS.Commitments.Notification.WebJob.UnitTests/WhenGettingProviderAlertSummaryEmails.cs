using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.PAS.Account.Api.Client;
using SFA.DAS.PAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenGettingProviderAlertSummaryEmails
    {
        private Mock<IApprenticeshipRepository> _apprenticeshipRepostory;

        private ProviderAlertSummaryEmailService _sut;

        private Mock<IPasAccountApiClient> _pasAccountService;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipRepostory = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 2,
                                              DataMismatchCount = 2,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 2
                                          }
                                  });

            _pasAccountService = new Mock<IPasAccountApiClient>();

            _pasAccountService.Setup(m => m.GetAccountUsers(12345))
                .ReturnsAsync(
                    new List<User>
                    {
                        new User
                        {
                            EmailAddress = "email@email.com",
                            DisplayName = "Tester son",
                            IsSuperUser = false,
                            ReceiveNotifications = true
                        }
                    });

            _sut = new ProviderAlertSummaryEmailService(
                _apprenticeshipRepostory.Object,
                Mock.Of<ICommitmentsLogger>(),
                _pasAccountService.Object
                );
        }

        [Test]
        public async Task ThenNoSummariesFound()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>(0));

            var emails = await _sut.GetEmails();

            emails.Count().Should().Be(0);
        }

        [Test]
        public async Task ThenFindingAlertButCantFindProviderUser()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 2,
                                              DataMismatchCount = 2,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 2
                                          },
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 1,
                                              DataMismatchCount = 1,
                                              ProviderId = 12346,
                                              ProviderName = "Test Provider 2",
                                              TotalCount = 2
                                          }
                                  });

            var emails = await _sut.GetEmails();

            emails.Count().Should().Be(1);
            var email = emails.Single();
            email.Tokens["provider_name"].Should().Be("Test Provider 1");
        }

        [Test]
        public async Task OneAlertForProvider()
        {
            var emails = await _sut.GetEmails();

            emails.Count().Should().Be(1);
            var email = emails.Single();

            email.Tokens["name"].Should().Be("Tester");
            email.Tokens["total_count_text"].Should().Be("are 2 apprentices");
            email.Tokens["provider_name"].Should().Be("Test Provider 1");
            email.Tokens["need_needs"].Should().Be("need");
            email.Tokens["changes_for_review"].Should().Be("* 2 apprentices with changes for review");
            email.Tokens["mismatch_changes"].Should().Be("* 2 apprentices with an ILR data mismatch");

            email.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }

        [Test]
        public async Task OneAlertForProviderWithSingular()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 1,
                                              DataMismatchCount = 1,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 1
                                          }
                                  });

            var emails = await _sut.GetEmails();

            emails.Count().Should().Be(1);
            var email = emails.Single();

            email.Tokens["name"].Should().Be("Tester");
            email.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            email.Tokens["provider_name"].Should().Be("Test Provider 1");
            email.Tokens["need_needs"].Should().Be("needs");
            email.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            email.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            email.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }

        [Test]
        public async Task WhenOneUserFromAccountsIsReturndBothGetsEmail()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 1,
                                              DataMismatchCount = 1,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 1
                                          }
                                  });

            _pasAccountService.Setup(m => m.GetAccountUsers(12345)).ReturnsAsync(
                new List<User>
                {
                    new User
                    {
                        EmailAddress = "first@email.COM",
                        DisplayName = "First Name",
                        ReceiveNotifications = true,
                        UserRef = "user1"
                    },
                    new User
                        {
                            EmailAddress = "second@email.COM",
                            DisplayName = "Second Name",
                            ReceiveNotifications = true,
                            UserRef = "user2"
                        }
                });

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(2);
            var first = emails[0];
            var second = emails[1];

            first.Tokens["name"].Should().Be("First");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");

            second.Tokens["name"].Should().Be("Second");
        }

        [Test]
        public async Task ShouldIgnoreSuperUsersEvenWithNotificationsOn()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 1,
                                              DataMismatchCount = 1,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 1
                                          }
                                  });
            _pasAccountService.Setup(m => m.GetAccountUsers(12345)).ReturnsAsync(
                new List<User>
                {
                    new User { EmailAddress = "super@email.COM", DisplayName = "super user", ReceiveNotifications = true, IsSuperUser = true, UserRef = "user1" },
                    new User { EmailAddress = "normal@ail.COM", DisplayName = "normal user", ReceiveNotifications = true, IsSuperUser = false, UserRef = "user2" }
                });

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(1);
            var first = emails[0];

            first.Tokens["name"].Should().Be("normal");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }

        [Test]
        public async Task WhenUserFromAccountsAPIHaveTurnedOffNotification()
        {
            _apprenticeshipRepostory.Setup(m => m.GetProviderApprenticeshipAlertSummary())
                .ReturnsAsync(new List<ProviderAlertSummary>
                                  {
                                      new ProviderAlertSummary
                                          {
                                              ChangesForReview = 1,
                                              DataMismatchCount = 1,
                                              ProviderId = 12345,
                                              ProviderName = "Test Provider 1",
                                              TotalCount = 1
                                          }
                                  });
            _pasAccountService.Setup(m => m.GetAccountUsers(12345)).ReturnsAsync(
                new List<User>
                {
                    new User { EmailAddress = "found-on@email.COM", DisplayName = "found-on", ReceiveNotifications = true, UserRef = "user1" },
                    new User { EmailAddress = "found-off@email.COM", DisplayName = "found-off", ReceiveNotifications = false, UserRef = "user2" }
                });

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(1);
            var first = emails[0];

            first.Tokens["name"].Should().Be("found-on");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }
    }
}
