using NUnit.Framework;
using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Services;
using Moq;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Types;
using System.Linq;
using SFA.DAS.Testing.Builders;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class FilterOutAcademicYearRollOverDataLocksTests
    {
        private Mock<ProviderCommitmentsDbContext> _dbContextMock;
        private FilterOutAcademicYearRollOverDataLocks _filter;
        private IFixture _fixture;
        public string LegalEntityIdentifier;
        public OrganisationType organisationType;
        public List<Apprenticeship> SeedApprenticeships;
        public List<DataLockStatus> DataLockRecordss;
        public List<ApprenticeshipUpdate> SeedApprenticeshipUpdates;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            LegalEntityIdentifier = "SC171417";
            organisationType = OrganisationType.CompaniesHouse;
            SeedApprenticeships = new List<Apprenticeship>();
            SeedApprenticeshipUpdates = new List<ApprenticeshipUpdate>();
            DataLockRecordss = new List<DataLockStatus>();

            _dbContextMock = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options)
            {
                CallBase = true
            };

            _filter = new FilterOutAcademicYearRollOverDataLocks(
               new Lazy<ProviderCommitmentsDbContext>(() => _dbContextMock.Object),
                Mock.Of<ILogger<FilterOutAcademicYearRollOverDataLocks>>());
        }

        [Test(Description = "No datalocks for apprenticeship so nothing to do")]
        public async Task WhenNoDataLocks()
        {
            _dbContextMock
                .Setup(context => context.DataLocks)
                .ReturnsDbSet(DataLockRecordss);

            _dbContextMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await _filter.Filter(123);

            _dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test(Description = "When has data locks but there are none with the same effective date then do nothing")]
        public async Task WhenNoDuplicateDatalocksForEffectiveDate()
        {
            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = new DateTime(2017, 7, 1), Status = Status.Pass },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2017", IlrEffectiveFromDate = new DateTime(2017, 8, 1), Status = Status.Pass }
            };

            _dbContextMock
               .Setup(context => context.DataLocks)
               .ReturnsDbSet(apprenticeshipDataLocks);

            _dbContextMock
             .Setup(context => context.DataLocks.Remove(It.IsAny<DataLockStatus>()));

            _dbContextMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await _filter.Filter(123);

            _dbContextMock
            .Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 4))), Times.Never);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date then delete the latest if it's for August price period.")]
        public async Task WhenHasDuplicateDatalocksForEffectiveDate()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 2000 },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1),  IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 3000},
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate,  IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000},
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate,  IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000}
            };

            _dbContextMock
             .Setup(context => context.DataLocks)
             .ReturnsDbSet(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _dbContextMock
            .Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 4))), Times.Once);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date but the price identifider for first is alphabetically laster than august one then delete the latest if it's for August price period.")]
        public async Task WhenHasDuplicateDatalocksForEffectiveDateButNonOverlapIsAlphabeticallyLater()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 2000 },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 3000 },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-22/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 }
            };

            _dbContextMock.Setup(context => context.DataLocks)
             .ReturnsDbSet(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _dbContextMock
           .Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 4))), Times.Once);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date but the price episode isn't august do nothing other than log an error")]
        public async Task WhenHasDuplicateDataLocksButLatestIsntAugust()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 2000 },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 3000 },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/09/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 }
            };

            _dbContextMock.Setup(context => context.DataLocks)
          .ReturnsDbSet(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _dbContextMock.Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 4))), Times.Never);
        }

        [Test(Description = "When there are duplicate datalocks with the same effective date but the price is not the same for the duplicates")]
        public async Task WhenHasDuplicateDataLocksButHasDifferentPrice()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/05/2017", IlrEffectiveFromDate = new DateTime(2017, 5, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 2000 },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/06/2017", IlrEffectiveFromDate = new DateTime(2017, 6, 1), IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 3000 },
                new DataLockStatus { DataLockEventId = 3, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/07/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 },
                new DataLockStatus { DataLockEventId = 4, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/09/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 5000 }
            };

            _dbContextMock.Setup(context => context.DataLocks).ReturnsDbSet(apprenticeshipDataLocks);

            await _filter.Filter(123);
            _dbContextMock.Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 4))), Times.Never);
        }

        [Test(Description = "When there data lock roll over events for next year it should delete these")]
        public async Task WhenHasDuplicateDataLocksButForNextYear()
        {
            DateTime duplicatIlreEffectiveFromDate = new DateTime(2017, 07, 01);

            List<DataLockStatus> apprenticeshipDataLocks = new List<DataLockStatus>
            {
                new DataLockStatus { DataLockEventId = 1, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/09/2017", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 },
                new DataLockStatus { DataLockEventId = 2, ApprenticeshipId = 123, PriceEpisodeIdentifier = "25-6-01/08/2018", IlrEffectiveFromDate = duplicatIlreEffectiveFromDate, IlrTrainingCourseCode = "2", IlrTrainingType = TrainingType.Standard, IlrActualStartDate = new DateTime(2017, 05, 01), IlrTotalCost = 4000 }
            };

            _dbContextMock.Setup(context => context.DataLocks).ReturnsDbSet(apprenticeshipDataLocks);

            await _filter.Filter(123);

            _dbContextMock.Verify(context => context.DataLocks.Remove(It.Is<DataLockStatus>((a => a.DataLockEventId == 2))), Times.Once);
        }
    }
}