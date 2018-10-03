using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingApprenticeshipDataLockProperties
    {
        private ApprenticeshipMapper _mapper;
        private Domain.Entities.Apprenticeship _apprenticeship;

        [SetUp]
        public void Setup()
        {
            _apprenticeship  = new Domain.Entities.Apprenticeship();
            _mapper = new ApprenticeshipMapper();
        }

        [Test]
        public void ThenAnUntriagedDataLockCourseIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock04,
                        TriageStatus = TriageStatus.Unknown
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsTrue(result.DataLockCourse);
        }

        [Test]
        public void ThenATriagedAsChangeDataLockCourseIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock04,
                        TriageStatus = TriageStatus.Change
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsFalse(result.DataLockCourse);
            Assert.IsTrue(result.DataLockCourseChangeTriaged);
        }


        [Test]
        public void ThenATriagedAsRestartDataLockCourseIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock04,
                        TriageStatus = TriageStatus.Restart
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsFalse(result.DataLockCourse);
            Assert.IsFalse(result.DataLockCourseChangeTriaged);
            Assert.IsTrue(result.DataLockCourseTriaged);
        }



        [Test]
        public void ThenAnUntriagedDataLockCourseAndPriceIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Unknown
                    },
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock03,
                        TriageStatus = TriageStatus.Unknown
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsTrue(result.DataLockCourse);
        }


        [Test]
        public void ThenAnUntriagedDataLockPriceOnlyIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Unknown
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsTrue(result.DataLockPrice);
        }


        [Test]
        public void ThenATriagedDataLockPriceOnlyIsMappedCorrectly()
        {
            _apprenticeship = new Domain.Entities.Apprenticeship
            {
                DataLocks = new List<DataLockStatusSummary>
                {
                    new DataLockStatusSummary
                    {
                        ErrorCode = DataLockErrorCode.Dlock07,
                        TriageStatus = TriageStatus.Change
                    }
                }
            };

            //Act
            var result = _mapper.MapFrom(_apprenticeship, CallerType.Employer);

            //Assert
            Assert.IsTrue(result.DataLockPriceTriaged);
        }
    }
}
