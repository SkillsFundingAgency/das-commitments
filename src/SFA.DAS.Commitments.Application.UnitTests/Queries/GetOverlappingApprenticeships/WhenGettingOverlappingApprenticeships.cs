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

            _handler = new GetOverlappingApprenticeshipsQueryHandler(_apprenticeshipRepository.Object, _validator.Object, new ApprenticeshipOverlapRules());
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var request = new GetOverlappingApprenticeshipsRequest { OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>()};

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
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2017,10,1),
                        DateTo = new DateTime(2017,11,1)
                   }
                }
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [TestCase("2018-04-01", "2018-06-30", Description = "Start and end date both disregarded")]
        [TestCase("2018-04-01", "2018-05-15", Description = "Start date disregarded")]
        [TestCase("2018-05-15", "2018-06-01", Description = "End date disregarded")]

        public async Task ThenTheOverlapCheckDisregardsDatesWithinTheSameMonth(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = startDate,
                        DateTo = endDate
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [TestCase("2017-01-01", "2017-12-31", Description = "Before any apprenticeships")]
        [TestCase("2021-01-01", "2021-12-31", Description = "After any apprenticeships")]
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = startDate,
                        DateTo = endDate
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
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "999999999",
                        DateFrom = new DateTime(2018,01,1),
                        DateTo = new DateTime(2018,12,31)
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
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,03,15),
                        DateTo = new DateTime(2018,05,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
        }

        [Test]
        public async Task ThenIfEndDateFallsWithinRangeOfExistingApprenticeshipThenIsOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                       Uln = "1234567890",
                        DateFrom = new DateTime(2018,05,15),
                        DateTo = new DateTime(2018,07,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(1, result.Data.Count);
        }

        [TestCase("2018-03-01", "2018-03-31", Description = "Dates contained within existing range - single month")]
        [TestCase("2020-03-15", "2020-09-15", Description = "Dates contained within existing range - longer duration")]
        [TestCase("2018-02-15", "2018-04-15", Description = "Same dates as existing range")]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping(DateTime startDate, DateTime endDate)
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = startDate,
                        DateTo = endDate
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
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,03,15),
                        DateTo = new DateTime(2018,07,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.AreEqual(2, result.Data.Count);
        }

        [Test]
        public async Task ThenIfDatesStraddleExistingApprenticeshipThenIsOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(CreateTestData());

            var request = new GetOverlappingApprenticeshipsRequest
            {
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,01,15),
                        DateTo = new DateTime(2018,05,15)
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
                OverlappingApprenticeshipRequests = new List<OverlappingApprenticeshipRequest>
                {
                   new OverlappingApprenticeshipRequest
                   {
                        ExcludeApprenticeshipId = 666,
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,02,15),
                        DateTo = new DateTime(2018,04,15)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        private static List<ApprenticeshipResult> CreateTestData()
        {
            var mockData = new List<ApprenticeshipResult>
            {
                CreateTestRecord("1234567890", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord("1234567890", new DateTime(2018,06,15), new DateTime(2018,08,15)),
                CreateTestRecord("1234567890", new DateTime(2018,12,01), new DateTime(2018,12,31)),
                CreateTestRecord("1234567890", new DateTime(2020,01,15), new DateTime(2020,12,15))
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
