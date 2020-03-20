using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions
{
    public class WhenGettingApprenticeshipsWithOrWithoutAlerts
    {
        private IQueryable<Apprenticeship> _apprenticeships;

        [SetUp]
        public void Arrange()
        {
            _apprenticeships = GetTestData();
        }

        [Test]
        public void ThenWillReturnApprenticeshipsWithAlerts()
        {
            //Act
            var result = _apprenticeships.WithAlerts(true).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
        }
        
        [Test]
        public void ThenWillReturnApprenticeshipsWithoutAlerts()
        {
            //Act
            var result = _apprenticeships.WithAlerts(false).ToList();

            //Assert
            Assert.AreEqual(5, result.Count);

            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual(4, result[1].Id);
            Assert.AreEqual(5, result[2].Id);
            Assert.AreEqual(6, result[3].Id);
            Assert.AreEqual(7, result[4].Id);
        }

        private static IQueryable<Apprenticeship> GetTestData()
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    Id = 1,
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus
                        {
                            IsResolved = false,
                            Status = Status.Fail,
                            EventStatus = EventStatus.New
                        }
                    }
                },
                new Apprenticeship {Id = 2},
                new Apprenticeship
                {
                    Id = 3,
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = ApprenticeshipUpdateStatus.Pending,
                            Originator = Originator.Provider
                        }
                    }
                },
                new Apprenticeship
                {
                    Id = 4,
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus
                        {
                            IsResolved = true,
                            Status = Status.Fail,
                            EventStatus = EventStatus.New
                        }
                    }
                },
                new Apprenticeship
                {
                    Id = 5,
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus
                        {
                            IsResolved = false,
                            Status = Status.Pass,
                            EventStatus = EventStatus.New
                        }
                    }
                },
                new Apprenticeship
                {
                    Id = 6,
                    DataLockStatus = new List<DataLockStatus>
                    {
                        new DataLockStatus
                        {
                            IsResolved = false,
                            Status = Status.Fail,
                            EventStatus = EventStatus.Removed
                        }
                    }
                },
                new Apprenticeship
                {
                    Id = 7,
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = ApprenticeshipUpdateStatus.Deleted,
                            Originator = Originator.Provider
                        }
                    }
                },
                new Apprenticeship
                {
                    Id = 8,
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>
                    {
                        new ApprenticeshipUpdate
                        {
                            Status = ApprenticeshipUpdateStatus.Pending,
                            Originator = Originator.Unknown
                        }
                    }
                },
            }.AsQueryable();
            return apprenticeships;
        }
    }
}
