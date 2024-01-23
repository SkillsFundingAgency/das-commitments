using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class ApprenticeshipDomainServiceTests
    {
        private ApprenticeshipDomainServiceTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ApprenticeshipDomainServiceTestsFixture();
        }

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenCorrectNumberOfEmployerNotifcationReturned(List<DataCases.Input> inputs, List<EmployerAlertSummaryNotification> expectedResults)
        {
            // Arrange
            foreach (var input in inputs)
            {
                _fixture
                    .WithApprenticeship(input.ApprenticeshipInput.AccountLegalEntityId, input.ApprenticeshipInput.LegalEntityId, input.ApprenticeshipInput.AccountId,
                        input.ApprenticeshipInput.CohortId, input.ApprenticeshipInput.ApprenticeshipId, input.ApprenticeshipInput.PaymentStatus, input.ApprenticeshipInput.Originator)
                    .WithDataLock(input.DataLockInput.DataLockStatusId, input.ApprenticeshipInput.ApprenticeshipId, input.DataLockInput.IsResolved, input.DataLockInput.IsExpired, input.DataLockInput.Status,
                        input.DataLockInput.EventStatus, input.DataLockInput.TriageStatus, input.DataLockInput.ErrorCode);
            }

            // Act
            var actualResults = await _fixture.GetEmployerAlertSummaryNotifications();

            // Assert
            actualResults.Should().HaveCount(expectedResults.Count);
        }

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenCorrectEmployerNotifcationReturned(List<DataCases.Input> inputs, List<EmployerAlertSummaryNotification> expectedResults)
        {
            // Arrange
            foreach (var input in inputs)
            {
                _fixture
                    .WithApprenticeship(input.ApprenticeshipInput.AccountLegalEntityId, input.ApprenticeshipInput.LegalEntityId, input.ApprenticeshipInput.AccountId,
                        input.ApprenticeshipInput.CohortId, input.ApprenticeshipInput.ApprenticeshipId, input.ApprenticeshipInput.PaymentStatus, input.ApprenticeshipInput.Originator)
                    .WithDataLock(input.DataLockInput.DataLockStatusId, input.ApprenticeshipInput.ApprenticeshipId, input.DataLockInput.IsResolved, input.DataLockInput.IsExpired, input.DataLockInput.Status,
                        input.DataLockInput.EventStatus, input.DataLockInput.TriageStatus, input.DataLockInput.ErrorCode);
            }

            // Act
            var actualResults = await _fixture.GetEmployerAlertSummaryNotifications();

            // Assert
            actualResults.Should().BeEquivalentTo(expectedResults);
        }

        public class DataCases : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                #region no notifications
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 0, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None },
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                    }
                };
                #endregion

                #region single provider notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 1, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                        }
                    }
                };
                #endregion

                #region single price triage notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 2, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification> 
                    { 
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 1, RestartRequestCount = 0
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 3, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock03 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 4, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock04 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 5, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock05 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                        }
                    }
                };
                #endregion

                #region single course triage notification
                yield return new object[]
                {
                    new List<Input>
                    {
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 0, LegalEntityId = "LE0", AccountId = 1000, CohortId = 0, ApprenticeshipId = 6, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 0, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1000", TotalCount = 1, ChangesForReviewCount = 0, RestartRequestCount = 1
                        }
                    }
                };
                #endregion

                #region multiple notifications
                yield return new object[]
                {
                   new List<Input>
                   {
                        // single provider notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 1, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 1, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },
                        // single price triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 2, LegalEntityId = "LE2", AccountId = 1002, CohortId = 2, ApprenticeshipId = 2, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 2, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 3, LegalEntityId = "LE3", AccountId = 1003, CohortId = 3, ApprenticeshipId = 3, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 3, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock03 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 4, LegalEntityId = "LE4", AccountId = 1004, CohortId = 4, ApprenticeshipId = 4, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 4, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock04 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 5, LegalEntityId = "LE5", AccountId = 1005, CohortId = 5, ApprenticeshipId = 5, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 5, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock05 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 6, LegalEntityId = "LE6", AccountId = 1006, CohortId = 6, ApprenticeshipId = 6, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 6, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 6, LegalEntityId = "LE6", AccountId = 1006, CohortId = 6, ApprenticeshipId = 7, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 7, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
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
                        }
                    }
                };
                #endregion

                #region multiple notifications for same employer account
                yield return new object[]
                {
                   new List<Input>
                   {
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 6, LegalEntityId = "LE6", AccountId = 1006, CohortId = 6, ApprenticeshipId = 6, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 6, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        },
                        // single course triage notification same employer account
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 6, LegalEntityId = "LE6", AccountId = 1006, CohortId = 6, ApprenticeshipId = 7, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 7, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1006", TotalCount = 2, ChangesForReviewCount = 0, RestartRequestCount = 2
                        }
                    }
                };
                #endregion

                #region multiple notifications of multiple types for multiple same employer account
                yield return new object[]
                {
                   new List<Input>
                   {
                        // single provider notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 1, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 1, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },

                        // single provider notification for same employer
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 2, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 2, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },
                        // single price triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 3, LegalEntityId = "LE3", AccountId = 1003, CohortId = 3, ApprenticeshipId = 3, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 3, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        },
                        // single price triage notification for same employer
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 3, LegalEntityId = "LE3", AccountId = 1003, CohortId = 3, ApprenticeshipId = 4, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 4, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 5, LegalEntityId = "LE5", AccountId = 1005, CohortId = 5, ApprenticeshipId = 5, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 5, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 5, LegalEntityId = "LE5", AccountId = 1005, CohortId = 5, ApprenticeshipId = 6, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 6, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
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
                    }
                };
                #endregion

                #region multiple notifications of multiple types for single same employer account
                yield return new object[]
                {
                   new List<Input>
                   {
                        // single provider notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 1, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 1, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },

                        // single provider notification for same employer
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 2, PaymentStatus = PaymentStatus.Active, Originator = Originator.Provider },
                            DataLockInput = new DataLockInput { DataLockStatusId = 2, IsResolved = true, IsExpired = true, Status = Status.Pass, EventStatus = EventStatus.Removed, TriageStatus = TriageStatus.Unknown, ErrorCode = DataLockErrorCode.None }
                        },
                        // single price triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 3, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 3, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        },
                        // single price triage notification for same employer
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 4, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 4, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Change, ErrorCode = DataLockErrorCode.Dlock07 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 5, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 5, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        },
                        // single course triage notification
                        new Input
                        {
                            ApprenticeshipInput =  new ApprenticeshipInput { AccountLegalEntityId = 1, LegalEntityId = "LE1", AccountId = 1001, CohortId = 1, ApprenticeshipId = 6, PaymentStatus = PaymentStatus.Active, Originator = Originator.Employer },
                            DataLockInput = new DataLockInput { DataLockStatusId = 6, IsResolved = false, IsExpired = false, Status = Status.Fail, EventStatus = EventStatus.New, TriageStatus = TriageStatus.Restart, ErrorCode = DataLockErrorCode.Dlock06 }
                        }
                    },
                    new List<EmployerAlertSummaryNotification>
                    {
                        new EmployerAlertSummaryNotification
                        {
                            EmployerHashedAccountId = "HSH1001", TotalCount = 6, ChangesForReviewCount = 4, RestartRequestCount = 2
                        }
                    }
                };
                #endregion
            }

            #region Test Data Classes
            public class Input
            {
                public ApprenticeshipInput ApprenticeshipInput { get; set; }
                public DataLockInput DataLockInput { get; set; }
            }

            public class ApprenticeshipInput
            {
                public long AccountLegalEntityId { get; set; }
                public string LegalEntityId { get; set; }
                public long AccountId { get; set; }
                public long CohortId { get; set; }
                public long ApprenticeshipId { get; set; }
                public PaymentStatus PaymentStatus {get; set;}
                public Originator Originator { get; set; }
            }

            public class DataLockInput
            {
                public long DataLockStatusId { get; set; }
                public bool IsResolved { get; set; }
                public bool IsExpired { get; set; }
                public Status Status { get; set; }
                public EventStatus EventStatus {get; set; }
                public TriageStatus TriageStatus { get; set; }
                public DataLockErrorCode ErrorCode { get; set; }
            }
            #endregion
        }

        public class ApprenticeshipDomainServiceTestsFixture
        {
            public List<Apprenticeship> SeedApprenticeships { get; }
            public List<DataLockStatus> SeedDataLocks { get; }
            
            private Mock<IEncodingService> _encodingService;

            public ApprenticeshipDomainServiceTestsFixture()
            {
                SeedApprenticeships = new List<Apprenticeship>();
                SeedDataLocks = new List<DataLockStatus>();
                
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(s => s.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns<long, EncodingType>((value, encodingType) => $"HSH{value}");
            }

            public Task<List<EmployerAlertSummaryNotification>> GetEmployerAlertSummaryNotifications()
            {
                return RunWithDbContext(dbContext =>
                {
                    var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                    var service = new ApprenticeshipDomainService(lazy, _encodingService.Object, Mock.Of<ILogger<ApprenticeshipDomainService>>());

                    return service.GetEmployerAlertSummaryNotifications();
                });
            }

            public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
            {
                var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                    .EnableSensitiveDataLogging()
                    .Options;

                using (var dbContext = new ProviderCommitmentsDbContext(options))
                {
                    dbContext.Database.EnsureCreated();
                    SeedData(dbContext);
                    return action(dbContext);
                }
            }

            private void SeedData(ProviderCommitmentsDbContext dbContext)
            {
                dbContext.Apprenticeships.AddRange(SeedApprenticeships);
                dbContext.DataLocks.AddRange(SeedDataLocks);
                dbContext.SaveChanges(true);
            }

            public ApprenticeshipDomainServiceTestsFixture WithApprenticeship(long accountLegalEntityId, string legalEntityId, long accountId, long cohortId, long apprenticeshipId, PaymentStatus paymentStatus, Originator originator)
            {
                var accountLegalEntity =
                    SeedApprenticeships.FirstOrDefault(p => p.Cohort.AccountLegalEntity.Id == accountLegalEntityId)?.Cohort?.AccountLegalEntity
                        ?? new AccountLegalEntity()
                            .Set(a => a.LegalEntityId, legalEntityId)
                            .Set(a => a.OrganisationType, OrganisationType.CompaniesHouse)
                            .Set(a => a.AccountId, accountId)
                            .Set(a => a.Id, accountLegalEntityId);
                    
                var cohort =
                    SeedApprenticeships.FirstOrDefault(p => p.Cohort.Id == cohortId)?.Cohort
                        ?? new Cohort()
                            .Set(c => c.Id, cohortId)
                            .Set(c => c.EmployerAccountId, accountId)
                            .Set(c => c.AccountLegalEntity, accountLegalEntity)
                            .Set(c => c.AccountLegalEntityId, accountLegalEntity.Id);

                var apprenticeship = new Apprenticeship()
                    .Set(s => s.Id, apprenticeshipId)
                    .Set(s => s.CommitmentId, cohort.Id)
                    .Set(s => s.Cohort, cohort)
                    .Set(s => s.PaymentStatus, paymentStatus)
                    .Set(s => s.PendingUpdateOriginator, originator);

                SeedApprenticeships.Add(apprenticeship);
                return this;
            }

            public ApprenticeshipDomainServiceTestsFixture WithDataLock(long dataLockStatusId, long apprenticeshipId, bool isResolved, bool isExpired, Status status, EventStatus eventStatus, TriageStatus triageStatus, DataLockErrorCode errorCode)
            {
                var dataLock = new DataLockStatus()
                    .Set(c => c.Id, dataLockStatusId)
                    .Set(c => c.ApprenticeshipId, apprenticeshipId)
                    .Set(c => c.IsResolved, isResolved)
                    .Set(c => c.IsExpired, isExpired)
                    .Set(c => c.Status, status)
                    .Set(c => c.EventStatus, eventStatus)
                    .Set(c => c.TriageStatus, triageStatus)
                    .Set(c => c.ErrorCode, errorCode);
               
                SeedDataLocks.Add(dataLock);
                return this;
            }
        }
    }
}
