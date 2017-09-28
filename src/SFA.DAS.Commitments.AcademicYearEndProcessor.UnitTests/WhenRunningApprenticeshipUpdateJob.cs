using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;
using Ploeh.AutoFixture;

using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.UnitTests
{
    [TestFixture]
    public class WhenRunningApprenticeshipUpdateJob
    {
        private Mock<ILog> _logger;
        private Mock<IAcademicYearDateProvider> _academicYearProvider;
        private Mock<IDataLockRepository> _dataLockRepository;
        private Mock<IApprenticeshipUpdateRepository> _apprenticeshipUpdateRepository;
        private Mock<ICurrentDateTime> _currentDateTime;

        private AcademicYearEndExpiryProcessor _sut;

        [SetUp]
        public void Arrange()
        {
            // ARRANGE
            _logger = new Mock<ILog>();
            _academicYearProvider = new Mock<IAcademicYearDateProvider>();
            _dataLockRepository = new Mock<IDataLockRepository>();
            _apprenticeshipUpdateRepository = new Mock<IApprenticeshipUpdateRepository>();
            _currentDateTime = new Mock<ICurrentDateTime>();

            _sut = new AcademicYearEndExpiryProcessor(
                _logger.Object, 
                _academicYearProvider.Object, 
                _dataLockRepository.Object, 
                _apprenticeshipUpdateRepository.Object,
                _currentDateTime.Object);

        }

        [Test]
        public async Task WhenNoUpdatesFound()
        {
            _apprenticeshipUpdateRepository.Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                .ReturnsAsync(new List<ApprenticeshipUpdate>());

            await _sut.RunApprenticeshipUpdateJob("jobId");


            _apprenticeshipUpdateRepository.Verify(m => m.GetExpiredApprenticeshipUpdates(It.IsAny<DateTime>()), Times.Exactly(2), failMessage: "Should call one time to get all updates and one to verify that all have been updated");
            _apprenticeshipUpdateRepository.Verify(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()), Times.Never, failMessage: "Should be called once for each update record");
        }

        [Test]
        public async Task WhenApprenticeshpUpdatesFound()
        {
            var records = 4;
            var apprenticeshipUpdates = new List<ApprenticeshipUpdate>();
            var fixture = new Fixture();
            fixture.AddManyTo(apprenticeshipUpdates, records);
            
            _apprenticeshipUpdateRepository.Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                .ReturnsAsync(apprenticeshipUpdates);

            _apprenticeshipUpdateRepository.Setup(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Callback(
                    () =>
                        {
                            // Setting data source to empty
                            _apprenticeshipUpdateRepository
                                .Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                                .ReturnsAsync(new List<ApprenticeshipUpdate>());
                        })
                .Returns(Task.FromResult(0));

            await _sut.RunApprenticeshipUpdateJob("jobId");

            _apprenticeshipUpdateRepository
                .Verify(m => m.GetExpiredApprenticeshipUpdates(It.IsAny<DateTime>()), Times.Exactly(2), 
                "Should call one time to get all updates and one to verify that all have been updated");
            _apprenticeshipUpdateRepository.Verify(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()), 
                Times.Exactly(records), 
                "Should be called once for each update record");

        }
    }
}
