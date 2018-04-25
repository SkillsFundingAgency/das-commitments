using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;

using NUnit.Framework;

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

        [Test]
        public async Task ShouldOnlyUpdateRecordsWithCostOrTrainingChanges()
        {
            var apprenticeshipUpdates = new List<ApprenticeshipUpdate>
                                            {
                                                new ApprenticeshipUpdate {  FirstName = "Abba1", Cost = null, TrainingCode = null, StartDate = null},
                                                new ApprenticeshipUpdate {  FirstName = "Abba2", Cost = 2000, TrainingCode = null, StartDate = null },
                                                new ApprenticeshipUpdate {  FirstName = "Abba3", Cost = null, TrainingCode = null, StartDate = null },
                                                new ApprenticeshipUpdate {  FirstName = "Abba4", Cost = null, TrainingCode = "123-1-1-", StartDate = null },
                                                new ApprenticeshipUpdate {  FirstName = "Abba5", Cost = 3000, TrainingCode = "123-1-1-", StartDate = null },
                                                new ApprenticeshipUpdate {  FirstName = "Abba5", Cost = null, TrainingCode = null, StartDate = new DateTime(DateTime.Now.Year, 06, 01)}

                                            };

            _apprenticeshipUpdateRepository.Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                .ReturnsAsync(apprenticeshipUpdates);

            _apprenticeshipUpdateRepository.Setup(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Callback(
                    () =>
                    {
                        // Setting data source to empty after the first call
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
                Times.Exactly(4),
                "Should be called once for each record with Cost or TrainingCode changes");

        }
    }
}
