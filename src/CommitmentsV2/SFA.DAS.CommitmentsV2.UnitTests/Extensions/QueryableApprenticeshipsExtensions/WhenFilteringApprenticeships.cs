using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions
{
    public class WhenFilteringApprenticeships
    {
        [Test]
        public void ThenShouldFilterEmployerNames()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = CreateAccountLegalEntity("no filter value")
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters {EmployerName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.AccountLegalEntity.Name.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterEmployerNamesEvenWhenSomeCohortDoNotExist()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = CreateAccountLegalEntity(filterValue)
                    }
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        AccountLegalEntity = CreateAccountLegalEntity("ACME Supplies")
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters{EmployerName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.AccountLegalEntity.Name.Equals(filterValue)));
        }

         [Test]
        public void ThenShouldFilterProviderNames()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        Provider = new Provider{Name = filterValue}
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        Provider = new Provider{Name = filterValue}
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        Provider = new Provider{Name = "no filter value"}
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters {ProviderName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.Provider.Name.Equals(filterValue)));
        }

        [Test]
        public void ThenFilteringOfProviderNamesWillStillWorkEvenWhenApprenticeshipCohortDoNotExist()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        Provider = new Provider{Name = filterValue}
                    }
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        Provider = new Provider{Name = "ACME Supplies"}
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters{ProviderName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.Provider.Name.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterCourseNames()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    CourseName = filterValue
                },
                new Apprenticeship
                {
                   CourseName = filterValue
                },
                new Apprenticeship
                {
                    CourseName = "no filter value"
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters {CourseName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.CourseName.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterCourseNamesEvenWhenSomeCourseNamesDoNotExist()
        {
            //Arrange
            const string filterValue = "Test Corp";

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    CourseName = filterValue
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    CourseName = "no filter value"
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters {CourseName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.CourseName.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterStatus()
        {
            //Arrange
            var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatus();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    PaymentStatus = filterValue
                },
                new Apprenticeship
                {
                    PaymentStatus = filterValue
                },
                new Apprenticeship
                {
                    PaymentStatus = PaymentStatus.Paused
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters
                {Status = ApprenticeshipStatus.Completed};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.PaymentStatus.Equals(filterValue)));
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
                new Apprenticeship
                {
                    Id = 1,
                    PaymentStatus = PaymentStatus.Completed
                },
                new Apprenticeship
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
            Assert.AreEqual(1, result.Count);
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
                new Apprenticeship
                {
                    Id = 1,
                    PaymentStatus = PaymentStatus.Completed
                },
                expectedApprenticeship,
                new Apprenticeship
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
            Assert.AreEqual(1, result.Count);
            result.Should().AllBeEquivalentTo(expectedApprenticeship);
        }

        [Test]
        public void ThenShouldFilterStatusEvenWhenSomePaymentStatusesDoNotExist()
        {
            //Arrange
            var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatus();

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    PaymentStatus = filterValue
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    PaymentStatus = PaymentStatus.Paused
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters
                {Status = ApprenticeshipStatus.Completed};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.PaymentStatus.Equals(filterValue)));
        }

         [Test]
        public void ThenShouldFilterStartDate()
        {
            //Arrange
            var filterValue = DateTime.Now;

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    StartDate = filterValue
                },
                new Apprenticeship
                {
                    StartDate = filterValue
                },
                new Apprenticeship
                {
                    StartDate = filterValue.AddMonths(1)
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters {StartDate = filterValue};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.StartDate.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterStartDateEvenWhenSomeStartDatesDoNotExist()
        {
            //Arrange
            var filterValue = DateTime.Now;

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    StartDate = filterValue
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    StartDate = filterValue.AddMonths(1)
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters {StartDate = filterValue};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.StartDate.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterEndDate()
        {
            //Arrange
            var filterValue = DateTime.Now;

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    EndDate = filterValue
                },
                new Apprenticeship
                {
                    EndDate = filterValue
                },
                new Apprenticeship
                {
                    EndDate = filterValue.AddMonths(1)
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters {EndDate = filterValue};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.EndDate.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterEndDateEvenWhenSomeEndDatesDoNotExist()
        {
            //Arrange
            var filterValue = DateTime.Now;

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    EndDate = filterValue
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    EndDate = filterValue.AddMonths(1)
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters {EndDate = filterValue};

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.EndDate.Equals(filterValue)));
        }

        [Test]
        public void ThenShouldFilterAccountLegalEntities()
        {
            //Arrange
            var filterValue = 123;

            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort()
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    Cohort = new Cohort { AccountLegalEntityId = filterValue }
                }
            }.AsQueryable();

            var filterValues = new ApprenticeshipSearchFilters { AccountLegalEntityId = filterValue };

            //Act
            var result = apprenticeships.Filter(filterValues).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.AccountLegalEntityId == filterValue));
        }

        [Test]
        public void ThenShouldNotFilterIfNoFilterValuesGiven()
        {
            //Arrange
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        //LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        //LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
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
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ThenShouldNotFilterIfNullGivenAsFilters()
        {
            //Arrange
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort()
                },
                new Apprenticeship
                {
                    Cohort = new Cohort()
                },
                new Apprenticeship
                {
                    Cohort = new Cohort()
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(null).ToList();

            //Assert
            Assert.AreEqual(3, result.Count);
        }

        private AccountLegalEntity CreateAccountLegalEntity(string name)
        {
            var account = new Account(1, "", "", name, DateTime.UtcNow);
            return new AccountLegalEntity(account, 1, 1, "", "", name, OrganisationType.CompaniesHouse, "",
                DateTime.UtcNow);
        }
    }
}
