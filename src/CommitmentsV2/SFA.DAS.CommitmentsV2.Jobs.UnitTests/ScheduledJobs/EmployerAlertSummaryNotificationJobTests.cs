using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.Testing.AutoFixture;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs
{
    public class EmployerAlertSummaryNotificationJobTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_AlertSummaryService_IsCalledToNotifyEmployers(
            [Frozen] Mock<IAlertSummaryService> alertSummaryService,
            EmployerAlertSummaryNotificationJob sut
            )
        {
            //Act
            await sut.Notify(null);

            //Assert
            alertSummaryService.Verify(m => m.SendEmployerAlertSummaryNotifications(), Times.Once);
        }
    }
}