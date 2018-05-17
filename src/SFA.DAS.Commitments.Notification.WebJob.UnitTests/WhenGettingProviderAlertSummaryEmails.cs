using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Notification.WebJob.EmailServices;
using SFA.DAS.PAS.Account.Api.Types;

using IAccountApiClient = SFA.DAS.PAS.Account.Api.Client.IAccountApiClient;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenGettingProviderAlertSummaryEmails
    {
        private Mock<IApprenticeshipRepository> _apprenticeshipRepostory;

        private Mock<IProviderEmailServiceWrapper> _providerUserService;

        private ProviderAlertSummaryEmailTemplateService _sut;

        private Mock<IAccountApiClient> _accountService;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipRepostory = new Mock<IApprenticeshipRepository>();
            _providerUserService = new Mock<IProviderEmailServiceWrapper>();
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

            _providerUserService.Setup(m => m.GetUsersAsync(12345))
                .ReturnsAsync(
                    new List<ProviderUser>
                        {
                            new ProviderUser
                                {
                                    Email = "email@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "Tester",
                                    Title = "Mr",
                                    Ukprn = 12345
                                }
                        });

            _accountService = new Mock<PAS.Account.Api.Client.IAccountApiClient>();

            _sut = new ProviderAlertSummaryEmailTemplateService(
                _apprenticeshipRepostory.Object,
                Mock.Of<ICommitmentsLogger>(),
                _providerUserService.Object,
                _accountService.Object
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

            _providerUserService.Setup(m => m.GetUsersAsync(12345))
                .ReturnsAsync(
                    new List<ProviderUser>
                        {
                            new ProviderUser
                                {
                                    Email = "notfound@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "NotFound",
                                    Title = "Mr",
                                    Ukprn = 12345
                                },
                            new ProviderUser
                                {
                                    Email = "found@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "Found",
                                    Title = "Mr",
                                    Ukprn = 12345
                                }
                        });

            _accountService.Setup(m => m.GetAccountUsers(12345)).ReturnsAsync(
                new List<User>
                {
                    new User
                        {
                            EmailAddress = "found@email.COM",
                            ReceiveNotifications = true,
                            UserRef = "user1"
                        }
                });

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(2);
            var first = emails[0];
            var second = emails[1];

            first.Tokens["name"].Should().Be("NotFound");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");

            second.Tokens["name"].Should().Be("Found");
        }

        [Test]
        public async Task WhenUserNotFoundInAccountsApi()
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

            _providerUserService.Setup(m => m.GetUsersAsync(12345))
                .ReturnsAsync(
                    new List<ProviderUser>
                        {
                            new ProviderUser
                                {
                                    Email = "notfound@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "NotFound",
                                    Title = "Mr",
                                    Ukprn = 12345
                                }
                        });

            _accountService.Setup(m => m.GetAccountUsers(12345)).Throws<HttpRequestException>();

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(1);
            var first = emails[0];

            first.Tokens["name"].Should().Be("NotFound");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"]
                .Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");            
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

            _providerUserService.Setup(m => m.GetUsersAsync(12345))
                .ReturnsAsync(
                    new List<ProviderUser>
                        {
                            new ProviderUser
                                {
                                    Email = "found-on@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "Found-ON",
                                    Title = "Mr",
                                    Ukprn = 12345
                                },
                            new ProviderUser
                                {
                                    Email = "found-off@email.com",
                                    FamilyName = "Testerson",
                                    GivenName = "Found-OFF",
                                    Title = "Mr",
                                    Ukprn = 12345
                                }
                        });

            _accountService.Setup(m => m.GetAccountUsers(12345)).ReturnsAsync(
                new List<User>
                {
                    new User { EmailAddress = "found-on@email.COM", ReceiveNotifications = true, UserRef = "user1" },
                    new User { EmailAddress = "found-off@email.COM", ReceiveNotifications = false, UserRef = "user2" }
                });

            var emails = (await _sut.GetEmails()).ToArray();

            emails.Length.Should().Be(1);
            var first = emails[0];

            first.Tokens["name"].Should().Be("Found-ON");
            first.Tokens["total_count_text"].Should().Be("is 1 apprentice");
            first.Tokens["provider_name"].Should().Be("Test Provider 1");
            first.Tokens["need_needs"].Should().Be("needs");

            first.Tokens["changes_for_review"].Should().Be("* 1 apprentice with changes for review");
            first.Tokens["mismatch_changes"].Should().Be("* 1 apprentice with an ILR data mismatch");

            first.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }
    }
}
