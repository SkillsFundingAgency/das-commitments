using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetActiveApprenticeshipsByUln
{
    [TestFixture]
    public class WhenGetActiveApprenticeshipsByUln
    {
        private GetActiveApprenticeshipsByUlnQueryHandler _handler;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<GetActiveApprenticeshipsByUlnValidator> _validator;
        private Mock<ICommitmentsLogger> _logger;

        private const string TestUln = "1234567890";

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<GetActiveApprenticeshipsByUlnValidator>();
            _validator.Setup(x => x.Validate(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .Returns(new ValidationResult());

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _apprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult>());

            _logger = new Mock<ICommitmentsLogger>();

            _handler = new GetActiveApprenticeshipsByUlnQueryHandler(_apprenticeshipRepository.Object, _validator.Object, _logger.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest
            {
                Uln = ""
            };

            await _handler.Handle(request);

            _validator.Verify(x => x.Validate(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveApprenticeshipsByUln()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest
            {
                Uln = TestUln
            };

            await _handler.Handle(request);

            _apprenticeshipRepository.Verify(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Test]
        public async Task ThenIgnoreRequestsWithoutUlns()
        {
            var request = new GetActiveApprenticeshipsByUlnRequest
            {
                Uln = null
            };

            await _handler.Handle(request);

            _apprenticeshipRepository.Verify(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<GetActiveApprenticeshipsByUlnRequest>()))
                .Returns(() =>
                    new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure("Error", "Error Message")
                    }));

            var request = new GetActiveApprenticeshipsByUlnRequest();

            Func<Task> act = async () => await _handler.Handle(request);
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenTheResponseIsMappedCorrectly()
        {
            var testRecord = CreateTestRecord(TestUln, new DateTime(2018, 02, 15), new DateTime(2018, 04, 15));

            _apprenticeshipRepository
                .Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<ApprenticeshipResult> { testRecord });

            var request = new GetActiveApprenticeshipsByUlnRequest { Uln = TestUln };

            var result = await _handler.Handle(request);

            var firstResult = result.Data.First();

            testRecord.Should().NotBeNull();
            testRecord.ShouldBeEquivalentTo(firstResult);
        }

        [Test]
        public async Task ThenLoggerIsCalledForEveryRecord()
        {
            var testRecords = CreateTestData(TestUln);

            _apprenticeshipRepository
                .Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(testRecords);

            var request = new GetActiveApprenticeshipsByUlnRequest { Uln = TestUln };

            await _handler.Handle(request);

            _logger.Verify(x => x.Info(It.IsAny<string>(), null, null, null, null, null, null), Times.Exactly(testRecords.Count));
        }

        private static List<ApprenticeshipResult> CreateTestData(string testUln)
        {
            var mockData = new List<ApprenticeshipResult>
            {
                CreateTestRecord(testUln, new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord(testUln, new DateTime(2018,06,15), new DateTime(2018,08,15)),
                CreateTestRecord(testUln, new DateTime(2018,12,01), new DateTime(2018,12,31)),
                CreateTestRecord(testUln, new DateTime(2020,01,15), new DateTime(2020,12,15)),
                CreateTestRecord(testUln, new DateTime(2021,03,15), new DateTime(2021,04,15)),
                CreateTestRecord(testUln, new DateTime(2021,09,15), new DateTime(2021,09,15))
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
