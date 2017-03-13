using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
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

            _handler = new GetOverlappingApprenticeshipsQueryHandler(_apprenticeshipRepository.Object, _validator.Object);
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

        [Test]
        public async Task ThenTheOverlapCheckDisregardsDatesWithinTheSameMonth()
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
                        DateFrom = new DateTime(2018,04,01),
                        DateTo = new DateTime(2018,06,30)
                   },
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,04,01),
                        DateTo = new DateTime(2018,05,15)
                   },
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2018,05,15),
                        DateTo = new DateTime(2018,06,01)
                   }
                }
            };

            //Act
            var result = await _handler.Handle(request);

            //Assert
            Assert.IsEmpty(result.Data);
        }

        [Test]
        public async Task ThenIfDatesDoNotFallWithinRangeOfExistingApprenticeshipThenNotOverlapping()
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
                        DateFrom = new DateTime(2017,01,1),
                        DateTo = new DateTime(2017,12,31)
                   },
                   new OverlappingApprenticeshipRequest
                   {
                        Uln = "1234567890",
                        DateFrom = new DateTime(2019,01,1),
                        DateTo = new DateTime(2019,12,31)
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

        [Test]
        public async Task ThenIfBothDatesFallWithinRangeOfSingleExistingApprenticeshipThenIsOverlapping()
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
                        DateFrom = new DateTime(2018,03,1),
                        DateTo = new DateTime(2018,03,31)
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

        private static List<ApprenticeshipResult> CreateTestData()
        {
            var mockData = new List<ApprenticeshipResult>
            {
                CreateTestRecord("1234567890", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord("1234567890", new DateTime(2018,06,15), new DateTime(2018,08,15))
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
