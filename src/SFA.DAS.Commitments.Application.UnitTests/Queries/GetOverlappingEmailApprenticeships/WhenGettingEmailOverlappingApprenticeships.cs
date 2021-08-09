using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;
using ApprenticeshipEmailOverlapValidationRequest = SFA.DAS.Commitments.Domain.Entities.ApprenticeshipEmailOverlapValidationRequest;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetOverlappingEmailApprenticeships
{
    [TestFixture]
    public class WhenGettingEmailOverlappingApprenticeships
    {
        private GetEmailOverlappingApprenticeshipsQueryHandler _handler;

        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<GetEmailOverlappingApprenticeshipsValidator> _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<GetEmailOverlappingApprenticeshipsValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<GetEmailOverlappingApprenticeshipsRequest>()))
                .Returns(new ValidationResult());

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>());

            _handler = new GetEmailOverlappingApprenticeshipsQueryHandler(_apprenticeshipRepository.Object, _validator.Object, Mock.Of<ICommitmentsLogger>());
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>()
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _validator.Verify(x => x.Validate(It.IsAny<GetEmailOverlappingApprenticeshipsRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveApprenticeshipsByUln()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                   new ApprenticeshipEmailOverlapValidationRequest
                   {
                        Email = "test@yahoo.com",
                        StartDate = new DateTime(2017,10,1),
                        EndDate = new DateTime(2017,11,1)
                   }
                }
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetEmaiOverlaps(It.IsAny<List<EmailToValidate>>()), Times.Once);
        }

        [Test]
        public async Task ThenIgnoreRequestsWithoutEmails()
        {
            //Arrange
            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                   new ApprenticeshipEmailOverlapValidationRequest
                   {
                        Email = null,
                        StartDate = new DateTime(2017,10,1),
                        EndDate = new DateTime(2017,11,1)
                   }
                }
            };

            //Act
            await _handler.Handle(request);

            //Assert
            _apprenticeshipRepository.Verify(x => x.GetEmaiOverlaps(It.IsAny<List<EmailToValidate>>()), Times.Never);
        }

        [Test]
        public async Task ThenIfNoEmailsMatchInputThenNotOverlapping()
        {
            //Arrange
            _apprenticeshipRepository.Setup(x => x.GetEmaiOverlaps(It.IsAny<List<EmailToValidate>>()))
                .ReturnsAsync(CreateSingleRecordTestData());

            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                   new ApprenticeshipEmailOverlapValidationRequest
                   {
                        Email = "test@yahoo.com",
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
            _apprenticeshipRepository.Setup(x => x.GetEmaiOverlaps(It.IsAny<List<EmailToValidate>>()))
              .ReturnsAsync(CreateTestData());

            var request = new GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = new List<ApprenticeshipEmailOverlapValidationRequest>
                {
                   new ApprenticeshipEmailOverlapValidationRequest
                   {
                        Email = "test@yahoo.com",
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

        private static List<OverlappingEmail> CreateSingleRecordTestData()
        {
            var mockData = new List<OverlappingEmail>
            {
                CreateTestRecord("test@yahoo.com", new DateTime(2018,02,15), new DateTime(2018,11,15))
            };

            return mockData;
        }
        private static List<OverlappingEmail> CreateTestData()
        {
            var mockData = new List<OverlappingEmail>
            {
                CreateTestRecord("test1@yahoo.com", new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord("test1@yahoo.com", new DateTime(2018,06,15), new DateTime(2018,08,15)),
                CreateTestRecord("test1@yahoo.com", new DateTime(2018,12,01), new DateTime(2018,12,31)),
                CreateTestRecord("test1@yahoo.com", new DateTime(2020,01,15), new DateTime(2020,12,15)),
                CreateTestRecord("test1@yahoo.com", new DateTime(2021,03,15), new DateTime(2021,04,15)),
                CreateTestRecord("test1@yahoo.com", new DateTime(2021,09,15), new DateTime(2021,09,15))
            };

            return mockData;
        }

        private static OverlappingEmail CreateTestRecord(string email, DateTime startDate, DateTime endDate)
        {
            return new OverlappingEmail
            {
                Email = email,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}