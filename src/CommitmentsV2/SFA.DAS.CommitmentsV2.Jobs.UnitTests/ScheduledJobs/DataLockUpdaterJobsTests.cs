using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs
{
    public class DataLockUpdaterJobsTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_DataLockUpdaterService_IsCalledToUpdateDatalocks(
            [Frozen] Mock<IDataLockUpdaterService> dataLockUpdaterService,
            DataLockUpdaterJobs sut
            )
        {
            //Act
            await sut.Update(null);

            //Assert
            dataLockUpdaterService.Verify(m => m.RunUpdate(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}