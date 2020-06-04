using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;
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
        private Mock<IMessagePublisher> _mockMessageBuilder;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
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
            _mockMessageBuilder = new Mock<IMessagePublisher>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();

            _sut = new AcademicYearEndExpiryProcessor(
                _logger.Object, 
                _academicYearProvider.Object, 
                _dataLockRepository.Object, 
                _apprenticeshipUpdateRepository.Object,
                _currentDateTime.Object,
                _mockMessageBuilder.Object,
                _mockApprenticeshipRepository.Object);

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
            var recordCount = 4;
            var apprenticeshipUpdates = new List<ApprenticeshipUpdate>();
            var apprenticeships = new List<Apprenticeship>();
            var fixture = new Fixture();
            fixture.AddManyTo(apprenticeshipUpdates, recordCount);
            apprenticeshipUpdates.ForEach(update =>
                apprenticeships.Add(
                    fixture.Build<Apprenticeship>()
                        .With(a => a.Id, update.ApprenticeshipId)
                        .Create()));
            
            _apprenticeshipUpdateRepository
                .Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                .ReturnsAsync(apprenticeshipUpdates);

            _apprenticeshipUpdateRepository
                .Setup(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()))
                .Callback(
                    () =>
                        {
                            // Setting data source to empty
                            _apprenticeshipUpdateRepository
                                .Setup(m => m.GetExpiredApprenticeshipUpdates(_currentDateTime.Object.Now))
                                .ReturnsAsync(new List<ApprenticeshipUpdate>());
                        })
                .Returns(Task.FromResult(0));

            _mockApprenticeshipRepository
                .Setup(repository =>
                    repository.GetApprenticeship(
                        It.IsIn(apprenticeshipUpdates.Select(update => update.ApprenticeshipId))))
                .ReturnsAsync((long apprenticeshipId) =>
                    apprenticeships.Single(apprenticeship => apprenticeship.Id == apprenticeshipId));
            
            await _sut.RunApprenticeshipUpdateJob("jobId");

            _apprenticeshipUpdateRepository
                .Verify(m => m.GetExpiredApprenticeshipUpdates(It.IsAny<DateTime>()), Times.Exactly(2), 
                "Should call one time to get all updates and one to verify that all have been updated");
            _apprenticeshipUpdateRepository.Verify(m => m.ExpireApprenticeshipUpdate(It.IsAny<long>()), 
                Times.Exactly(recordCount), 
                "Should be called once for each update record");
            apprenticeshipUpdates.ForEach(update =>
            {
                var apprenticeship = apprenticeships.Single(a => a.Id == update.ApprenticeshipId);
                _mockMessageBuilder.Verify(m =>
                    m.PublishAsync(It.Is<ApprenticeshipUpdateCancelled>(cancelled =>
                        cancelled.ApprenticeshipId == apprenticeship.Id &&
                        cancelled.AccountId == apprenticeship.EmployerAccountId &&
                        cancelled.ProviderId == apprenticeship.ProviderId)),
                    "Should be called once for each update record, with correct params");
            });
        }

        [Test, AutoData]
        public async Task ShouldOnlyUpdateRecordsWithCostOrTrainingChanges(
            Apprenticeship apprenticeship)
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

            _mockApprenticeshipRepository
                .Setup(repository => repository.GetApprenticeship(It.IsAny<long>()))
                .ReturnsAsync(apprenticeship);

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
