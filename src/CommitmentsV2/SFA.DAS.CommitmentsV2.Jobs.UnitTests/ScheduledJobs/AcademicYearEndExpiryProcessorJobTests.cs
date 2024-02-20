using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs;

[TestFixture]
public class WhenRunningJob
{
    [Test, MoqAutoData]
    public async Task ShouldRunAllTasks(
        [Frozen] Mock<ICurrentDateTime> currentDateTimeMock,
        [Frozen] Mock<IAcademicYearDateProvider> academicYearDateProvider,
        [Frozen] Mock<IAcademicYearEndExpiryProcessorService> academicYearProcessorMock,
        AcademicYearEndExpiryProcessorJob sut)
    {
        var academicYearStart = GetCurrentAcademicYearStartDate();
        currentDateTimeMock.SetupGet(m => m.UtcNow).Returns(academicYearStart.AddMonths(3));
        academicYearDateProvider.SetupGet(m => m.LastAcademicYearFundingPeriod).Returns(new DateTime(academicYearStart.Year, 10, 19));

        await sut.Run(null);

        academicYearProcessorMock.Verify(m => m.ExpireApprenticeshipUpdates(It.IsAny<string>()), Times.Once);
        academicYearProcessorMock.Verify(m => m.ExpireDataLocks(It.IsAny<string>()), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(true, false, 1, "ChangeOfCircs")]
    [MoqInlineAutoData(false, true, 1, "DataLocks")]
    [MoqInlineAutoData(true, true, 2, "ChangeOfCircs")]
    [MoqInlineAutoData(true, true, 2, "DataLocks")]
    public async Task WhenOneJobThrowsAnException_ThenLogJobIdentifier(
        bool expireApprenticeshipUpdatesFail,
        bool expireDatalocksFail,
        int expectedLogErrorTimes,
        string expectedJobIdentier,
        [Frozen] Mock<ICurrentDateTime> currentDateTimeMock,
        [Frozen] Mock<IAcademicYearDateProvider> academicYearDateProvider,
        [Frozen] Mock<IAcademicYearEndExpiryProcessorService> academicYearProcessorMock,
        [Frozen] Mock<ILogger<AcademicYearEndExpiryProcessorJob>> loggerMock,
        AcademicYearEndExpiryProcessorJob sut)
    {
        var academicYearStart = GetCurrentAcademicYearStartDate();
        currentDateTimeMock.SetupGet(m => m.UtcNow).Returns(academicYearStart.AddMonths(3));
        academicYearDateProvider.SetupGet(m => m.LastAcademicYearFundingPeriod).Returns(new DateTime(academicYearStart.Year, 10, 19));

        if (expireApprenticeshipUpdatesFail)
        {
            academicYearProcessorMock.Setup(m => m.ExpireApprenticeshipUpdates(It.IsAny<string>())).ThrowsAsync(new Exception("This is an expire apprenticeship update error"));
        }

        if (expireDatalocksFail)
        {
            academicYearProcessorMock.Setup(m => m.ExpireDataLocks(It.IsAny<string>())).ThrowsAsync(new Exception("This is an expire data lock error"));
        }

        await sut.Run(null);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(expectedLogErrorTimes));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(expectedJobIdentier)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static DateTime GetCurrentAcademicYearStartDate()
    {
        var now = DateTime.UtcNow;
        var cutoffUtc = new DateTime(now.Year, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var fundingPeriodEnd = now >= cutoffUtc ? cutoffUtc : new DateTime(now.Year - 1, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        return fundingPeriodEnd;
    }
}