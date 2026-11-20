using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs;

public class EmployerAlertSummaryNotificationJobTests
{
    [Test, MoqAutoData]
    public async Task Then_The_EmployerAlertSummaryEmailService_IsCalledToNotifyEmployers(
        [Frozen] Mock<IEmployerAlertSummaryEmailService> alertSummaryService,
        EmployerAlertSummaryNotificationJob sut)
    {
        await sut.Notify(null);

        alertSummaryService.Verify(m => m.SendEmployerAlertSummaryNotifications(), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_An_On_Time_Run_Sends(
        [Frozen] Mock<IEmployerAlertSummaryEmailService> alertSummaryService,
        EmployerAlertSummaryNotificationJob sut)
    {
        await sut.Notify(new TimerInfo(new NextDaySchedule(), new ScheduleStatus(), false));

        alertSummaryService.Verify(m => m.SendEmployerAlertSummaryNotifications(), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_A_Past_Due_Run_Does_Not_Send(
        [Frozen] Mock<IEmployerAlertSummaryEmailService> alertSummaryService,
        EmployerAlertSummaryNotificationJob sut)
    {
        await sut.Notify(new TimerInfo(new NextDaySchedule(), new ScheduleStatus(), true));

        alertSummaryService.Verify(m => m.SendEmployerAlertSummaryNotifications(), Times.Never);
    }

    private sealed class NextDaySchedule : TimerSchedule
    {
        public override bool AdjustForDST => false;

        public override DateTime GetNextOccurrence(DateTime now) => now.AddDays(1);
    }
}