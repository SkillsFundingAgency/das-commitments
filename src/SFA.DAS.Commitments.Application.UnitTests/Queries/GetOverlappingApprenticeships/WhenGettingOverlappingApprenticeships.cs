using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;

using ApprenticeshipOverlapValidationRequest = SFA.DAS.Commitments.Domain.Entities.ApprenticeshipOverlapValidationRequest;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetOverlappingApprenticeships
{
    [TestFixture]
    public class WhenGettingOverlappingApprenticeships
    {
        private GetOverlappingApprenticeshipsQueryHandler _handler;

        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<GetOverlappingApprenticeshipsValidator> _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<GetOverlappingApprenticeshipsValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .Returns(new ValidationResult());

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>());

            _handler = new GetOverlappingApprenticeshipsQueryHandler(_apprenticeshipRepository.Object, _validator.Object, new ApprenticeshipOverlapRules(), Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>()
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveApprenticeshipsByUln()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = new DateTime(2017,10,1),
                        EndDate = new DateTime(2017,11,1)
                   }
                }
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Test]
        public async Task ThenIgnoreRequestsWithoutUlns()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = null,
                        StartDate = new DateTime(2017,10,1),
                        EndDate = new DateTime(2017,11,1)
                   }
                }
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [TestCase("2017-01-01", "2017-12-31", Description = "Before any apprenticeships")]
        [TestCase("2022-01-01", "2022-12-31", Description = "After any apprenticeships")]
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = startDate,
                        EndDate = endDate
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [Test]
        public async Task ThenIfNoUlnsMatchInputThenNotOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "999999999",
                        StartDate = new DateTime(2018,01,1),
                        EndDate = new DateTime(2018,06,30)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [Test]
        public async Task ThenIfStartDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = new DateTime(2018,03,15),
                        EndDate = new DateTime(2018,12,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(ValidationFailReason.OverlappingStartDate, result.Data[0].ValidationFailReason);
        }

        [Test]
        public async Task ThenIfEndDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = new DateTime(2018,01,15),
                        EndDate = new DateTime(2018,03,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(ValidationFailReason.OverlappingEndDate, result.Data[0].ValidationFailReason);
        }

        [TestCase("2018-04-01", "2018-06-30", Description = "Start and end date both disregarded")]
        [TestCase("2018-04-01", "2018-05-15", Description = "Start date disregarded")]
        [TestCase("2018-05-15", "2018-06-01", Description = "End date disregarded")]
        [TestCase("2018-02-15", "2018-02-16", Description = "Start/end same month disregarded")]
        [TestCase("2021-09-15", "2021-09-15", Description = "Start/end same month as single-month active record disregarded")]
        [TestCase("2021-09-01", "2021-10-15", Description = "Start month overlaps single-month active record disregarded")]
        [TestCase("2021-08-15", "2021-09-15", Description = "End month overlaps single-month active record disregarded")]
        public async Task ThenTheOverlapCheckDisregardsDatesWithinTheSameMonth(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = startDate,
                        EndDate = endDate
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [TestCase("2018-06-01", "2018-06-30", Description = "Dates contained within existing range - single month")]
        [TestCase("2018-06-15", "2018-07-15", Description = "Dates contained within existing range - two months")]
        [TestCase("2018-03-15", "2018-09-15", Description = "Dates contained within existing range - longer duration")]
        [TestCase("2018-02-15", "2018-11-15", Description = "Same dates as existing range")]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = startDate,
                        EndDate = endDate
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
        }

        [Test]
        public async Task ThenIfDatesFallWithinRangeOfDifferentExistingApprenticeshipThenOverlapsBoth()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = new DateTime(2018,03,15),
                        EndDate = new DateTime(2018,07,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(2, result.Data.Count);
        }

        [TestCase("2018-01-15", "2018-05-15", Description = "Straightforward straddle")]
        [TestCase("2017-12-15", "2018-04-15", Description = "Partial straddle - end")]
        [TestCase("2017-02-15", "2018-05-15", Description = "Partial straddle - start")]
        [TestCase("2019-12-01", "2020-12-01", Description = "Possible straddle or end date within")]
        [TestCase("2021-08-01", "2021-10-01", Description = "Straddle around single-month active record")]
        public async Task ThenIfDatesStraddleExistingApprenticeshipThenIsOverlapping(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        Uln = "1234567890",
                        StartDate = startDate,
                        EndDate = endDate
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
        }

        [Test]
        public async Task ThenAnExistingApprenticeshipShouldNotBeConsideredAsOverlappingWithItself()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult
                    {
                        Id = 666,
                        EmployerAccountId = 1,
                        LegalEntityName = "Test Corp",
                        ProviderId = 999,
                        Uln = "1234567890",
                        StartDate = new DateTime(2018,03,15),
                        EndDate = new DateTime(2018,05,15)
                    }
                });

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        ApprenticeshipId = 666,
                        Uln = "1234567890",
                        StartDate = new DateTime(2018,02,15),
                        EndDate = new DateTime(2018,04,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [Test]
        public async Task ThenCheckingAgainstAStoppedApprenticeshipShouldCompareTheStoppedDate()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult
                    {
                        Id = 666,
                        EmployerAccountId = 1,
                        LegalEntityName = "Test Corp",
                        ProviderId = 999,
                        Uln = "123",
                        StartDate = new DateTime(2018,03, 01),
                        EndDate = new DateTime(2018,07,01),
                        StopDate = new DateTime(2018,05,01),
                        PaymentStatus = PaymentStatus.Withdrawn
                    }
                });

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        ApprenticeshipId = 2345,
                        Uln = "123",
                        StartDate = new DateTime(2018,06,01),
                        EndDate = new DateTime(2018,09,01)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [Test]
        public async Task ThenApprenticeshipStoppedBeforeStartedShouldBeIgnored()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult
                    {
                        Id = 666,
                        EmployerAccountId = 1,
                        LegalEntityName = "Test Corp",
                        ProviderId = 999,
                        Uln = "123",
                        StartDate = new DateTime(2018,01,01),
                        EndDate = new DateTime(2018,06,01),
                        StopDate = new DateTime(2018,01,01),
                        PaymentStatus = PaymentStatus.Withdrawn
                    }
                });

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<ApprenticeshipOverlapValidationRequest>
                {
                   new ApprenticeshipOverlapValidationRequest
                   {
                        ApprenticeshipId = 2345,
                        Uln = "123",
                        StartDate = new DateTime(2017,12,01),
                        EndDate = new DateTime(2018,09,01)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        /// <summary>
        /// Creates a single apprenticeship running Feb-Nov 2018
        /// </summary>
        /// <returns></returns>
        private static List<ApprenticeshipResult> CreateSingleRecordTestData()
        {
            var mockData = new List<ApprenticeshipResult>
            {
                CreateTestRecord("1234567890", new DateTime(2018,02,15), new DateTime(2018,11,15))
            };

            return mockData;
        }

        /// <summary>
        /// Creates a multi-record set of test data - 
        /// Feb-Apr 18, Jun-Aug 18, Dec 18, Jan-Dec 20, Mar-Apr 21, Sep 21
        /// </summary>
        /// <returns></returns>
        private static List<ApprenticeshipResult> CreateTestData()
        {
            var mockData = new List<ApprenticeshipResult>
            {
                CreateTestRecord("1234567890", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord("1234567890", new DateTime(2018,06,15), new DateTime(2018,08,15)),
                CreateTestRecord("1234567890", new DateTime(2018,12,01), new DateTime(2018,12,31)),
                CreateTestRecord("1234567890", new DateTime(2020,01,15), new DateTime(2020,12,15)),
                CreateTestRecord("1234567890", new DateTime(2021,03,15), new DateTime(2021,04,15)),
                CreateTestRecord("1234567890", new DateTime(2021,09,15), new DateTime(2021,09,15))
            };

            return mockData;
        }

        private static ApprenticeshipResult CreateTestRecord(string uln, DateTime startDate, DateTime endDate)
        {
            return new ApprenticeshipResult
            {
                EmployerAccountId = 1,
                LegalEntityName = "Test Corp",
                ProviderId = 999,
                Uln = uln,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
