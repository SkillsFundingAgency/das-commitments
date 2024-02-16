using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions;

public class WhenFilteringApprenticeships
{
    [Test]
    public void ThenShouldFilterEmployerNames()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort
                {
                    AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    AccountLegalEntity = CreateAccountLegalEntity("no filter value")
                }
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { EmployerName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.Cohort.AccountLegalEntity.Name.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterEmployerNamesEvenWhenSomeCohortDoNotExist()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort
                {
                    AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                }
            },
            new(),
            new()
            {
                Cohort = new Cohort
                {
                    AccountLegalEntity = CreateAccountLegalEntity("ACME Supplies")
                }
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { EmployerName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.Cohort.AccountLegalEntity.Name.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterProviderNames()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort
                {
                    Provider = new Provider{Name = filterValue}
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    Provider = new Provider{Name = filterValue}
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    Provider = new Provider{Name = "no filter value"}
                }
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { ProviderName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.Cohort.Provider.Name.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenFilteringOfProviderNamesWillStillWorkEvenWhenApprenticeshipCohortDoNotExist()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort
                {
                    Provider = new Provider{Name = filterValue}
                }
            },
            new(),
            new()
            {
                Cohort = new Cohort
                {
                    Provider = new Provider{Name = "ACME Supplies"}
                }
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { ProviderName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.Cohort.Provider.Name.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterCourseNames()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                CourseName = filterValue
            },
            new()
            {
                CourseName = filterValue
            },
            new()
            {
                CourseName = "no filter value"
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { CourseName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.CourseName.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterCourseNamesEvenWhenSomeCourseNamesDoNotExist()
    {
        //Arrange
        const string filterValue = "Test Corp";

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                CourseName = filterValue
            },
            new(),
            new()
            {
                CourseName = "no filter value"
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters { CourseName = filterValue }).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.CourseName.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterStatus()
    {
        //Arrange
        var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatus();

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                PaymentStatus = filterValue
            },
            new()
            {
                PaymentStatus = filterValue
            },
            new()
            {
                PaymentStatus = PaymentStatus.Paused
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters
            { Status = ApprenticeshipStatus.Completed };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.PaymentStatus.Equals(filterValue)), Is.True);
    }

    [Test]
    public void Then_If_Filtering_By_Waiting_To_Start_Adds_Date_Range()
    {
        //Arrange
        var expectedApprenticeship = new Apprenticeship
        {
            Id = 3,
            PaymentStatus = PaymentStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(1)
        };
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Id = 1,
                PaymentStatus = PaymentStatus.Completed
            },
            new()
            {
                Id = 2,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            expectedApprenticeship
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters
            { Status = ApprenticeshipStatus.WaitingToStart };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        result.Should().AllBeEquivalentTo(expectedApprenticeship);
    }

    [Test]
    public void Then_If_Filtering_By_Live_Adds_Date_Range()
    {
        //Arrange
        var expectedApprenticeship = new Apprenticeship
        {
            Id = 2,
            PaymentStatus = PaymentStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Id = 1,
                PaymentStatus = PaymentStatus.Completed
            },
            expectedApprenticeship,
            new()
            {
                Id = 3,
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(1)
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters
            { Status = ApprenticeshipStatus.Live };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        result.Should().AllBeEquivalentTo(expectedApprenticeship);
    }

    [Test]
    public void ThenShouldFilterStatusEvenWhenSomePaymentStatusesDoNotExist()
    {
        //Arrange
        var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatus();

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                PaymentStatus = filterValue
            },
            new(),
            new()
            {
                PaymentStatus = PaymentStatus.Paused
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters
            { Status = ApprenticeshipStatus.Completed };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.PaymentStatus.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterStartDate()
    {
        //Arrange
        var filterValue = DateTime.Now;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                StartDate = filterValue
            },
            new()
            {
                StartDate = filterValue
            },
            new()
            {
                StartDate = filterValue.AddMonths(1)
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { StartDate = filterValue };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.StartDate.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterStartDateEvenWhenSomeStartDatesDoNotExist()
    {
        //Arrange
        var filterValue = DateTime.Now;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                StartDate = filterValue
            },
            new(),
            new()
            {
                StartDate = filterValue.AddMonths(1)
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { StartDate = filterValue };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.StartDate.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterEndDate()
    {
        //Arrange
        var filterValue = DateTime.Now;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                EndDate = filterValue
            },
            new()
            {
                EndDate = filterValue
            },
            new()
            {
                EndDate = filterValue.AddMonths(1)
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { EndDate = filterValue };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(a => a.EndDate.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterEndDateEvenWhenSomeEndDatesDoNotExist()
    {
        //Arrange
        var filterValue = DateTime.Now;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                EndDate = filterValue
            },
            new(),
            new()
            {
                EndDate = filterValue.AddMonths(1)
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { EndDate = filterValue };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.EndDate.Equals(filterValue)), Is.True);
    }

    [Test]
    public void ThenShouldFilterAccountLegalEntities()
    {
        //Arrange
        var filterValue = 123;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort()
            },
            new(),
            new()
            {
                Cohort = new Cohort { AccountLegalEntityId = filterValue }
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { AccountLegalEntityId = filterValue };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.Cohort.AccountLegalEntityId == filterValue), Is.True);
    }

    [Test]
    public void ThenShouldFilterStartDateFrom()
    {
        //Arrange
        var filterValue = DateTime.Now.AddDays(-10).Date;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort(),
                StartDate = DateTime.Now.AddDays(-10).Date
            },
            new(),
            new()
            {
                Cohort = new Cohort(),
                StartDate = DateTime.Now.AddDays(-11).Date
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { StartDateRange = new DateRange { From = filterValue } };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.StartDate.Value.Date >= filterValue), Is.True);
    }

    [Test]
    public void ThenShouldFilterStartDateTo()
    {
        //Arrange
        var filterValue = DateTime.Now.AddDays(10).Date;

        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort(),
                StartDate = DateTime.Now.AddDays(10).Date
            },
            new(),
            new()
            {
                Cohort = new Cohort(),
                StartDate = DateTime.Now.AddDays(11).Date
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters { StartDateRange = new DateRange { To = filterValue } };

        //Act
        var result = apprenticeships.Filter(filterValues).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.StartDate.Value.Date <= filterValue), Is.True);
    }

    [TestCase(Alerts.IlrDataMismatch, "1", true)]
    [TestCase(Alerts.ChangesPending, "2", true)]
    [TestCase(Alerts.ChangesRequested, "3", true)]
    [TestCase(Alerts.ChangesForReview, "4", true)]
    [TestCase(Alerts.ConfirmDates, "5", false)]
    public void ThenShouldFilterAlert(Alerts alert, string validApprenticeshipUln, bool isProvider)
    {
        //Arrange
        var apprenticeships = CreateApprenticeships();
        var filterValues = new ApprenticeshipSearchFilters
        {
            Alert = alert
        };

        //Act
        var result = apprenticeships.Filter(filterValues, isProvider).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.Uln.Equals(validApprenticeshipUln)), Is.True);
    }

    [Test]
    public void ThenShouldNotFilterIfNoFilterValuesGiven()
    {
        //Arrange
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort
                {
                    //LegalEntityName = "Test Corp"
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    //LegalEntityName = "Test Corp"
                }
            },
            new()
            {
                Cohort = new Cohort
                {
                    //LegalEntityName = "ACME Supplies"
                }
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(new ApprenticeshipSearchFilters()).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void ThenShouldNotFilterIfNullGivenAsFilters()
    {
        //Arrange
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Cohort = new Cohort()
            },
            new()
            {
                Cohort = new Cohort()
            },
            new()
            {
                Cohort = new Cohort()
            }
        }.AsQueryable();

        //Act
        var result = apprenticeships.Filter(null).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void ThenShouldFilterApprenticeshipConfirmationStatus()
    {
        //Arrange
        var apprenticeships = new List<Apprenticeship>
        {
            // 1 Confirmed
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = DateTime.Now
                }
            },

            // 2 Unconfirmed
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null,
                    ConfirmationOverdueOn = null
                }
            },
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null,
                    ConfirmationOverdueOn = DateTime.Now.AddDays(2)
                }
            },

            // 3 overdue
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null,
                    ConfirmationOverdueOn = DateTime.Now.AddDays(-2)
                }
            },
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null,
                    ConfirmationOverdueOn = DateTime.Now.AddDays(-3)
                }
            },
            new()
            {
                Email = "a@a.com",
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null,
                    ConfirmationOverdueOn = DateTime.Now.AddDays(-4)
                }
            },

            // 4 NA
            new()
            {
                Email = null,
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null
                }
            },
            new()
            {
                Email = null,
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null
                }
            },
            new()
            {
                Email = null,
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null
                }
            },
            new()
            {
                Email = null,
                ApprenticeshipConfirmationStatus = new ApprenticeshipConfirmationStatus
                {
                    ApprenticeshipConfirmedOn = null
                }
            }
        }.AsQueryable();

        //Act
        var resultConfirmed = apprenticeships.Filter(new ApprenticeshipSearchFilters { ApprenticeConfirmationStatus = ConfirmationStatus.Confirmed }).ToList();
        var resultUnconfirmed = apprenticeships.Filter(new ApprenticeshipSearchFilters { ApprenticeConfirmationStatus = ConfirmationStatus.Unconfirmed }).ToList();
        var resultOverdue = apprenticeships.Filter(new ApprenticeshipSearchFilters { ApprenticeConfirmationStatus = ConfirmationStatus.Overdue }).ToList();
        var resultNA = apprenticeships.Filter(new ApprenticeshipSearchFilters { ApprenticeConfirmationStatus = ConfirmationStatus.NA }).ToList();

        Assert.Multiple(() =>
        {
            //Assert
            Assert.That(resultConfirmed, Has.Count.EqualTo(1));
            Assert.That(resultUnconfirmed, Has.Count.EqualTo(2));
            Assert.That(resultOverdue, Has.Count.EqualTo(3));
            Assert.That(resultNA, Has.Count.EqualTo(4));
        });
    }

    [Test]
    public void ThenShouldNotReturnExpiredDataLock()
    {
        //Arrange
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Uln =  "1",
                DataLockStatus = new List<DataLockStatus>
                {
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false,
                        IsExpired = true
                    },
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false,
                        IsExpired = true
                    },
                }
            },
            new()
            {
                Uln =  "2",
                DataLockStatus = new List<DataLockStatus>
                {
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false,
                        IsExpired = false
                    },
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false,
                        IsExpired = true
                    },
                }
            }
        }.AsQueryable();

        var filterValues = new ApprenticeshipSearchFilters
        {
            Alert = Alerts.ChangesPending
        };

        //Act
        var result = apprenticeships.Filter(filterValues, true).ToList();

        //Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.All(a => a.Uln == "2" && a.DataLockStatus.Any(x => !x.IsExpired)), Is.True);
    }

    private static AccountLegalEntity CreateAccountLegalEntity(string name)
    {
        var account = new Account(1, "", "", name, DateTime.UtcNow);
        return new AccountLegalEntity(account, 1, 1, "", "", name, OrganisationType.CompaniesHouse, "",
            DateTime.UtcNow);
    }

    private static IQueryable<Apprenticeship> CreateApprenticeships()
    {
        var apprenticeships = new List<Apprenticeship>
        {
            new()
            {
                Uln =  "1",
                DataLockStatus = new List<DataLockStatus>
                {
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Unknown,
                        IsResolved = false
                    },
                }
            },
            new()
            {
                Uln =  "2",
                DataLockStatus = new List<DataLockStatus>
                {
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false
                    },
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Change,
                        IsResolved = false
                    },
                }
            },
            new()
            {
                Uln =  "3",
                DataLockStatus = new List<DataLockStatus>
                {
                    new()
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Restart,
                        IsResolved = false
                    },
                }
            },
            new()
            {
                Uln =  "4",
                ApprenticeshipUpdate = new List<ApprenticeshipUpdate>{
                    new()
                    {
                        Originator = Originator.Employer,
                        Status = ApprenticeshipUpdateStatus.Pending
                    }
                }
            },
            new()
            {
                Uln =  "5",
                OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest> {
                    new()
                    {
                        Status = OverlappingTrainingDateRequestStatus.Pending,
                    }
                }
            },
            new()
            {
                Uln =  "100"
            },
        }.AsQueryable();

        return apprenticeships;
    }
}