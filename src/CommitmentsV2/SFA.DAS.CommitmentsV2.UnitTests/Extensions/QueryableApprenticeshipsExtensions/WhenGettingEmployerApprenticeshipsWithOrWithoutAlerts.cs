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
    public class WhenGettingEmployerApprenticeshipsWithOrWithoutAlerts
    {
        private IQueryable<Apprenticeship> _apprenticeships;
        private readonly ApprenticeshipSearchParameters _parameters = new ApprenticeshipSearchParameters { EmployerAccountId = 1 };

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
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(3, result[1].Id);
            Assert.AreEqual(10, result[2].Id);
        }

        [Test]
        public void ThenWillReturnApprenticeshipsWithoutAlerts()
        {
            //Act
            var result = _apprenticeships.WithAlerts(false, _parameters).ToList();

            //Assert
            Assert.AreEqual(6, result.Count);

            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual(4, result[1].Id);
            Assert.AreEqual(5, result[2].Id);
            Assert.AreEqual(6, result[3].Id);
            Assert.AreEqual(7, result[4].Id);
            Assert.AreEqual(9, result[5].Id);
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
                            EventStatus = EventStatus.New,
                            TriageStatus = TriageStatus.Change
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
                new Apprenticeship
                {
                    Id = 9,
                    DataLockStatus = new List<DataLockStatus>
                        {
                            new DataLockStatus
                            {
                                ErrorCode = DataLockErrorCode.Dlock03,
                                TriageStatus = TriageStatus.Unknown,
                                IsResolved = false
                            }
                        }
                },
                new Apprenticeship
                {
                    Id = 10,
                    OverlappingTrainingDateRequests = new List<OverlappingTrainingDateRequest>
                        {
                            new OverlappingTrainingDateRequest
                            {
                                Status = OverlappingTrainingDateRequestStatus.Pending,
                            }
                        }
                }
            }.AsQueryable();
            return apprenticeships;
        }
    }
}