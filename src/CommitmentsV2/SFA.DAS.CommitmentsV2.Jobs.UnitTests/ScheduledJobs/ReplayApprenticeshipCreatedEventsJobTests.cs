using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Models;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs;

public class ReplayApprenticeshipCreatedEventsJobTests
{
    [Test]
    public async Task Then_It_Does_Not_Process_When_No_Pending_Files()
    {
        var fileStore = new Mock<IReplayInputFileStore>();
        fileStore.Setup(x => x.GetPendingFiles()).ReturnsAsync([]);
        var replayService = new Mock<IReplayApprenticeshipCreatedEventsService>();

        var job = new ReplayApprenticeshipCreatedEventsJob(
            Mock.Of<ILogger<ReplayApprenticeshipCreatedEventsJob>>(),
            fileStore.Object,
            replayService.Object);

        await job.Replay(null);

        fileStore.Verify(x => x.GetPendingFiles(), Times.Once);
        replayService.Verify(x => x.ReplayFromFile(It.IsAny<ReplayInputFile>()), Times.Never);
    }

    [Test]
    public async Task Then_It_Processes_And_Archives_All_Files()
    {
        var files = new[]
        {
            new ReplayInputFile { Name = "a.csv", FullPath = "a.csv", Content = "1,2" },
            new ReplayInputFile { Name = "b.csv", FullPath = "b.csv", Content = "3,4" }
        };

        var fileStore = new Mock<IReplayInputFileStore>();
        fileStore.Setup(x => x.GetPendingFiles()).ReturnsAsync(files);

        var replayService = new Mock<IReplayApprenticeshipCreatedEventsService>();
        var job = new ReplayApprenticeshipCreatedEventsJob(
            Mock.Of<ILogger<ReplayApprenticeshipCreatedEventsJob>>(),
            fileStore.Object,
            replayService.Object);

        await job.Replay(null);

        replayService.Verify(x => x.ReplayFromFile(It.IsAny<ReplayInputFile>()), Times.Exactly(2));
        fileStore.Verify(x => x.ArchiveProcessedFile(It.IsAny<ReplayInputFile>()), Times.Exactly(2));
    }
}
