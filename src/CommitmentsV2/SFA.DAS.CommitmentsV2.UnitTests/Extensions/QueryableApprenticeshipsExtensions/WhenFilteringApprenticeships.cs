using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtension
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
                        LegalEntityName = filterValue
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = filterValue
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "no filter value"
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters {EmployerName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.LegalEntityName.Equals(filterValue)));
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
                        LegalEntityName = filterValue
                    }
                },
                new Apprenticeship(),
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "ACME Supplies"
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(new ApprenticeshipSearchFilters{EmployerName = filterValue}).ToList();

            //Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(a => a.Cohort.LegalEntityName.Equals(filterValue)));
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
            var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatuses().First();

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
        public void ThenShouldFilterStatusEvenWhenSomePaymentStatusesDoNotExist()
        {
            //Arrange
            var filterValue = ApprenticeshipStatus.Completed.MapToPaymentStatuses().First();

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
        public void ThenShouldNotFilterIfNoFilterValuesGiven()
        {
            //Arrange
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "ACME Supplies"
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
                    Cohort = new Cohort
                    {
                        LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "Test Corp"
                    }
                },
                new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        LegalEntityName = "ACME Supplies"
                    }
                }
            }.AsQueryable();

            //Act
            var result = apprenticeships.Filter(null).ToList();

            //Assert
            Assert.AreEqual(3, result.Count);
        }
    }
}
