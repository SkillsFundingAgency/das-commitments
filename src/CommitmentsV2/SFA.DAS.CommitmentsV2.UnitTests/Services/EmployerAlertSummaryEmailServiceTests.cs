using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Services;
using System.Collections;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class EmployerAlertSummaryEmailServiceTests
    {
        private EmployerAlertSummaryEmailServiceTestsFixture _fixture;
        public const string EmployerCommitmentsBaseUrl = "https://approvals.ResourceEnvironmentName-eas.apprenticeships.education.gov.uk/";

        [SetUp]
        public void SetUp()
        {
            _fixture = new EmployerAlertSummaryEmailServiceTestsFixture();
        }

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenAccountReponseRetreivedForEachEmployerNotification(DataCases.Input input, List<DataCases.Output> outputs)
        {
            // Arrange
            _fixture
                .WithEmployerAlertSummaryNotifications(input.EmployerAlertSummaryNotifications)
                .WithAccountResponse(input.AccountResponses);

            // Act
            await _fixture.SendEmployerAlertSummaryNotifications();

            // Assert
            _fixture.VerifyAccountReponses(outputs);
        }

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenSendEmailToEmployerCommandSentForEachEmployerNotification(DataCases.Input input, List<DataCases.Output> outputs)
        {
            // Arrange
            _fixture
                .WithEmployerAlertSummaryNotifications(input.EmployerAlertSummaryNotifications)
                .WithAccountResponse(input.AccountResponses);

            // Act
            await _fixture.SendEmployerAlertSummaryNotifications();

            // Assert
            _fixture.VerifySendEmailToEmployerCommandSent(outputs);
        }

        public class DataCases : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                #region no notifications
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                        }
                    },
                    new List<Output>
                    {
                    }
                };
                #endregion

                #region single provider notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", $"* 1 apprentice with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region single price triage notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", $"* 1 apprentice with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1000,
                                HashedAccountId = "HSH1000",
                                DasAccountName = "FIRST ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1000,
                            HashedAccountId = "HSH1000",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIRST ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1000/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1000" }
                            }
                        }
                    }
                };
                #endregion

                #region multiple notifications
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1001", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1002", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1003", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1004", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1005", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1006", TotalCount = 2, ChangesForReviewCount = 0, RestartRequestCount = 2
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1007", TotalCount = 3, ChangesForReviewCount = 2, RestartRequestCount = 1
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1001,
                                HashedAccountId = "HSH1001",
                                DasAccountName = "ONE ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1002,
                                HashedAccountId = "HSH1002",
                                DasAccountName = "TWO ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1003,
                                HashedAccountId = "HSH1003",
                                DasAccountName = "THREE ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1004,
                                HashedAccountId = "HSH1004",
                                DasAccountName = "FOUR ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1005,
                                HashedAccountId = "HSH1005",
                                DasAccountName = "FIVE ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1006,
                                HashedAccountId = "HSH1006",
                                DasAccountName = "SIX ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1007,
                                HashedAccountId = "HSH1007",
                                DasAccountName = "SEVEN ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1001,
                            HashedAccountId = "HSH1001",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "ONE ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", $"* 1 apprentice with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1001/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1001" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1002,
                            HashedAccountId = "HSH1002",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "TWO ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", $"* 1 apprentice with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1002/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1002" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1003,
                            HashedAccountId = "HSH1003",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "THREE ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1003/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1003" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1004,
                            HashedAccountId = "HSH1004",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FOUR ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1004/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1004" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1005,
                            HashedAccountId = "HSH1005",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "is 1 apprentice" },
                                { "account_name", "FIVE ACCOUNT" },
                                { "need_needs", "needs" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1005/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1005" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1006,
                            HashedAccountId = "HSH1006",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 2 apprentices" },
                                { "account_name", "SIX ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 2 apprentices with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1006/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1006" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1007,
                            HashedAccountId = "HSH1007",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 3 apprentices" },
                                { "account_name", "SEVEN ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", $"* 2 apprentices with changes for review" },
                                { "requested_changes", $"* 1 apprentice with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1007/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1007" }
                            }
                        }
                    }
                };
                #endregion

                #region multiple notifications for same employer account
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1006", TotalCount = 2, ChangesForReviewCount = 0, RestartRequestCount = 2
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1006,
                                HashedAccountId = "HSH1006",
                                DasAccountName = "SIX ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1006,
                            HashedAccountId = "HSH1006",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 2 apprentices" },
                                { "account_name", "SIX ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 2 apprentices with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1006/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1006" }
                            }
                        }
                    }
                };
                #endregion

                #region multiple notifications of multiple types for multiple same employer account
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1001", TotalCount = 2, ChangesForReviewCount = 2, RestartRequestCount = 0
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1003", TotalCount = 2, ChangesForReviewCount = 2, RestartRequestCount = 0
                            },
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1005", TotalCount = 2, ChangesForReviewCount = 0, RestartRequestCount = 2
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1001,
                                HashedAccountId = "HSH1001",
                                DasAccountName = "ONE ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1003,
                                HashedAccountId = "HSH1003",
                                DasAccountName = "THREE ACCOUNT"
                            },
                            new AccountResponse
                            {
                                AccountId = 1005,
                                HashedAccountId = "HSH1005",
                                DasAccountName = "FIVE ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1001,
                            HashedAccountId = "HSH1001",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 2 apprentices" },
                                { "account_name", "ONE ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", $"* 2 apprentices with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1001/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1001" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1003,
                            HashedAccountId = "HSH1003",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 2 apprentices" },
                                { "account_name", "THREE ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", $"* 2 apprentices with changes for review" },
                                { "requested_changes", "" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1003/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1003" }
                            }
                        },
                        new Output
                        {
                            AccountId = 1005,
                            HashedAccountId = "HSH1005",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", "are 2 apprentices" },
                                { "account_name", "FIVE ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", "" },
                                { "requested_changes", $"* 2 apprentices with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1005/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1005" }
                            }
                        }
                    }
                };
                #endregion

                #region multiple notifications of multiple types for single same employer account
                yield return new object[]
                {
                    new Input
                    {
                        EmployerAlertSummaryNotifications = new List<EmployerAlertSummaryNotification>
                        {
                            new EmployerAlertSummaryNotification
                            {
                                EmployerHashedAccountId = "HSH1001", TotalCount = 6, ChangesForReviewCount = 4, RestartRequestCount = 2
                            }
                        },
                        AccountResponses = new List<AccountResponse>
                        {
                            new AccountResponse
                            {
                                AccountId = 1001,
                                HashedAccountId = "HSH1001",
                                DasAccountName = "ONE ACCOUNT"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1001,
                            HashedAccountId = "HSH1001",
                            Tokens = new Dictionary<string, string>
                            {
                                { "total_count_text", $"are 6 apprentices" },
                                { "account_name", "ONE ACCOUNT" },
                                { "need_needs", "need" },
                                { "changes_for_review", $"* 4 apprentices with changes for review" },
                                { "requested_changes", $"* 2 apprentices with requested changes" },
                                { "link_to_mange_apprenticeships", $"{EmployerCommitmentsBaseUrl}/HSH1001/apprentices" },
                                { "link_to_unsubscribe", $"/settings/notifications/unsubscribe/HSH1001" }
                            }
                        }
                    }
                };
                #endregion
            }

            #region Test Data Classes
            public class Input
            {
                public List<EmployerAlertSummaryNotification> EmployerAlertSummaryNotifications { get; internal set; }
                public List<AccountResponse> AccountResponses { get; internal set; }
            }

            public class Output
            {
                public int AccountId { get; internal set; }
                public string HashedAccountId { get; internal set; }
                public Dictionary<string, string> Tokens { get; internal set; }
            }

            #endregion
        }

        public class EmployerAlertSummaryEmailServiceTestsFixture
        {
            private Mock<IMessageSession> _messageSession;
            private Mock<IApprovalsOuterApiClient> _approvalsOuterApiClient;
            private Mock<IApprenticeshipDomainService> _apprenticeshipDomainService;
            private static CommitmentsV2Configuration commitmentsV2Configuration;

            public EmployerAlertSummaryEmailServiceTestsFixture()
            {
                _apprenticeshipDomainService = new Mock<IApprenticeshipDomainService>();
                _messageSession = new Mock<IMessageSession>();
                _approvalsOuterApiClient = new Mock<IApprovalsOuterApiClient>();
                commitmentsV2Configuration = new CommitmentsV2Configuration()
                {
                    EmployerCommitmentsBaseUrl = EmployerCommitmentsBaseUrl
                };
            }

            public async Task SendEmployerAlertSummaryNotifications()
            {
                var service = new EmployerAlertSummaryEmailService(_apprenticeshipDomainService.Object, _messageSession.Object, _approvalsOuterApiClient.Object, Mock.Of<ILogger<EmployerAlertSummaryEmailService>>(), commitmentsV2Configuration);
                await service.SendEmployerAlertSummaryNotifications();
            }

            public EmployerAlertSummaryEmailServiceTestsFixture WithEmployerAlertSummaryNotifications(List<EmployerAlertSummaryNotification> employerAlertSummaryNotifications)
            {
                _apprenticeshipDomainService.Setup(m => m.GetEmployerAlertSummaryNotifications()).ReturnsAsync(employerAlertSummaryNotifications);
                return this;
            }

            public EmployerAlertSummaryEmailServiceTestsFixture WithAccountResponse(List<AccountResponse> accountResponses)
            {
                _approvalsOuterApiClient.Setup(x => x.Get<AccountResponse>(It.IsAny<GetAccountRequest>())).ReturnsAsync((GetAccountRequest request) =>
                    accountResponses.FirstOrDefault(p => p.HashedAccountId == request.AccountHashedId));

                return this;
            }

            public void VerifyAccountReponses(List<DataCases.Output> outputs)
            {
                if (outputs.Any())
                {
                    foreach (var output in outputs)
                    {
                        _approvalsOuterApiClient.Verify(m => m.Get<AccountResponse>(It.Is<GetAccountRequest>(p => p.AccountHashedId == output.HashedAccountId)), Times.Once);
                    }
                }
                else
                {
                    _approvalsOuterApiClient.Verify(m => m.Get<AccountResponse>(It.IsAny<GetAccountRequest>()), Times.Never);
                }
            }

            public void VerifySendEmailToEmployerCommandSent(List<DataCases.Output> outputs)
            {
                if (outputs.Any())
                {
                    foreach (var output in outputs)
                    {
                        _messageSession.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(p =>
                            p.AccountId == output.AccountId &&
                            p.Template == "EmployerAlertSummaryNotification" &&
                            p.Tokens.SequenceEqual(output.Tokens) &&
                            p.NameToken == "name"), It.IsAny<SendOptions>()), Times.Once);
                    }
                }
                else
                {
                    _messageSession.Verify(m => m.Send(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<SendOptions>()), Times.Never);
                }
            }
        }
    }
}
