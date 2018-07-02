using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprenticeshipsByUln
{
    [TestFixture]
    public class WhenGetApprenticeshipsByUln
    {
        private GetApprenticeshipsByUlnQueryHandler _sut;
        private Mock<IApprenticeshipRepository> _apprenticeshipRepository;
        private Mock<ICommitmentsLogger> _logger;
        private Mock<IUlnValidator> _ulnValidator;

        private const string TestUln = "1000201367";

        [SetUp]
        public void Arrange()
        {
            _ulnValidator = new Mock<IUlnValidator>();

            _apprenticeshipRepository = new Mock<IApprenticeshipRepository>();

            _apprenticeshipRepository
                .Setup(x => x.GetApprenticeshipsByUln(It.IsAny<string>()))
                .ReturnsAsync(CreateTestData(TestUln));

            _logger = new Mock<ICommitmentsLogger>();

            _sut = new GetApprenticeshipsByUlnQueryHandler(_apprenticeshipRepository.Object, _ulnValidator.Object, _logger.Object);
        }

        [Test]
        public async Task ThenTheRequestIsValidated()
        {
            var request = new GetApprenticeshipsByUlnRequest
            {
                Uln = TestUln
            };

            _ulnValidator
                .Setup(x => x.Validate(request.Uln))
                .Returns(UlnValidationResult.Success);

            await _sut.Handle(request);

            _ulnValidator.Verify(x => x.Validate(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledToRetrieveApprenticeshipsByUln()
        {
            var request = new GetApprenticeshipsByUlnRequest
            {
                Uln = TestUln
            };

            var result = await _sut.Handle(request);

            _apprenticeshipRepository.Verify(x => x.GetApprenticeshipsByUln(It.IsAny<string>()), Times.Once);

            result.Apprenticeships.ShouldAllBeEquivalentTo(CreateTestData(TestUln).Apprenticeships);
            result.TotalCount.ShouldBeEquivalentTo(6);
        }

        [Test]
        public void ThenIfTheRequestIsInvalidThenAValidationExceptionIsThrownAndIsRepositoryNeverCalled()
        {
            var request = new GetApprenticeshipsByUlnRequest();

            Func<Task> act = async () => await _sut.Handle(request);
            act.ShouldThrow<ValidationException>();

            _apprenticeshipRepository.Verify(x => x.GetApprenticeshipsByUln(It.IsAny<string>()), Times.Never);
        }

        private ApprenticeshipsResult CreateTestData(string testUln)
        {
            var mockData = new List<Apprenticeship>
            {
                CreateTestRecord(testUln, new DateTime(2018,02,15), new DateTime(2018,04,15)),
                CreateTestRecord(testUln, new DateTime(2018,06,15), new DateTime(2018,08,15)),
                CreateTestRecord(testUln, new DateTime(2018,12,01), new DateTime(2018,12,31)),
                CreateTestRecord(testUln, new DateTime(2020,01,15), new DateTime(2020,12,15)),
                CreateTestRecord(testUln, new DateTime(2021,03,15), new DateTime(2021,04,15)),
                CreateTestRecord(testUln, new DateTime(2021,09,15), new DateTime(2021,09,15))
            };

            return new ApprenticeshipsResult
            {
                Apprenticeships = mockData,
                TotalCount = mockData.Count()
            };
        }

        private Apprenticeship CreateTestRecord(string uln, DateTime startDate, DateTime endDate)
        {
            return new Apprenticeship
            {
                EmployerAccountId = 1,
                LegalEntityName = "Test Corp",
                ProviderId = 999,
                ULN = uln,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
