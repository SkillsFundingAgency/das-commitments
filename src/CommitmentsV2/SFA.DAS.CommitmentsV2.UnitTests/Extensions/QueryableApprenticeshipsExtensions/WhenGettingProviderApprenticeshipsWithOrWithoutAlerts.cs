using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions
{
    public class WhenGettingProviderApprenticeshipsWithOrWithoutAlerts
    {
        private IQueryable<Apprenticeship> _apprenticeships;
        private readonly ApprenticeshipSearchParameters _parameters = new ApprenticeshipSearchParameters { ProviderId = 1 };

        [SetUp]
        public void Arrange()
        {
            _apprenticeships = GetTestData();
        }

        [Test]
        public void ThenWillReturnApprenticeshipsWithAlerts()
        {
            //Act
            var result = _apprenticeships.WithAlerts(true, _parameters).ToList();

            //Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo(1));
                Assert.That(result[1].Id, Is.EqualTo(3));
            });
        }
        
        [Test]
        public void ThenWillReturnApprenticeshipsWithoutAlerts()
        {
            //Act
            var result = _apprenticeships.WithAlerts(false, _parameters).ToList();

            //Assert
            Assert.That(result, Has.Count.EqualTo(5));

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo(2));
                Assert.That(result[1].Id, Is.EqualTo(4));
                Assert.That(result[2].Id, Is.EqualTo(5));
                Assert.That(result[3].Id, Is.EqualTo(6));
                Assert.That(result[4].Id, Is.EqualTo(7));
            });
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
