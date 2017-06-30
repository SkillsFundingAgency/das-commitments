using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Data;

namespace SFA.DAS.Commitments.Notification.WebJob.UnitTests
{
    [TestFixture]
    public class WhenGettingProviderEmails
    {
        private Mock<IApprenticeshipRepository> _apprenticeshipRepostory;

        private Mock<IProviderEmailServiceWrapper> _providerUserService;

        private ProviderEmailTemplatesService _sut;

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

            _sut = new ProviderEmailTemplatesService(
                _apprenticeshipRepostory.Object,
                Mock.Of<ICommitmentsLogger>(),
                _providerUserService.Object
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

            email.Tokens["changes_for_review"].Should().Be("* 2 with changes for review");
            email.Tokens["mismatch_changes"].Should().Be("* 2 with an ILR data mismatch");

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

            email.Tokens["changes_for_review"].Should().Be("* 1 with changes for review");
            email.Tokens["mismatch_changes"].Should().Be("* 1 with an ILR data mismatch");

            email.Tokens["link_to_mange_apprenticeships"].Should().Be("12345/apprentices/manage/all?RecordStatus=ChangesForReview&RecordStatus=IlrDataMismatch&RecordStatus=ChangeRequested");
        }
    }
}
