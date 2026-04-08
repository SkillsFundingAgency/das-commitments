using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.Services;

public class ReplayInputFileStoreTests
{
    [Test]
    public async Task Then_GetPendingFiles_Returns_Only_Csv_Files()
    {
        // Arrange
        var blobStorageService = new Mock<IBlobStorageService>();
        blobStorageService
            .Setup(x => x.ListBlobsAsync("commitments-replay-input", "pending/"))
            .ReturnsAsync(["pending/a.csv", "pending/b.txt", "pending/c.CSV"]);
        blobStorageService
            .Setup(x => x.GetBlobAsync("commitments-replay-input", "pending/a.csv"))
            .ReturnsAsync(BinaryData.FromString("1,2"));
        blobStorageService
            .Setup(x => x.GetBlobAsync("commitments-replay-input", "pending/c.CSV"))
            .ReturnsAsync(BinaryData.FromString("3,4"));

        var sut = new ReplayInputFileStore(
            blobStorageService.Object,
            Mock.Of<ILogger<ReplayInputFileStore>>());

        // Act
        var result = await sut.GetPendingFiles();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(x => x.Name == "a.csv" && x.FullPath == "pending/a.csv" && x.Content == "1,2");
        result.Should().ContainSingle(x => x.Name == "c.CSV" && x.FullPath == "pending/c.CSV" && x.Content == "3,4");
        blobStorageService.Verify(x => x.GetBlobAsync("commitments-replay-input", "pending/b.txt"), Times.Never);
    }

    [Test]
    public async Task Then_GetPendingFiles_Ensures_Container_Before_Listing()
    {
        // Arrange
        var blobStorageService = new Mock<IBlobStorageService>();
        var sequence = new MockSequence();

        blobStorageService
            .InSequence(sequence)
            .Setup(x => x.EnsureContainerExistsAsync("commitments-replay-input"))
            .Returns(Task.CompletedTask);
        blobStorageService
            .InSequence(sequence)
            .Setup(x => x.ListBlobsAsync("commitments-replay-input", "pending/"))
            .ReturnsAsync([]);

        var sut = new ReplayInputFileStore(
            blobStorageService.Object,
            Mock.Of<ILogger<ReplayInputFileStore>>());

        // Act
        await sut.GetPendingFiles();

        // Assert
        blobStorageService.Verify(x => x.EnsureContainerExistsAsync("commitments-replay-input"), Times.Once);
        blobStorageService.Verify(x => x.ListBlobsAsync("commitments-replay-input", "pending/"), Times.Once);
    }

    [Test]
    public async Task Then_ArchiveProcessedFile_Uploads_To_Archive_And_Deletes_Source()
    {
        // Arrange
        var blobStorageService = new Mock<IBlobStorageService>();
        var sut = new ReplayInputFileStore(
            blobStorageService.Object,
            Mock.Of<ILogger<ReplayInputFileStore>>());

        var replayInputFile = new ReplayInputFile
        {
            Name = "input.csv",
            FullPath = "pending/input.csv",
            Content = "123,456"
        };

        // Act
        await sut.ArchiveProcessedFile(replayInputFile);

        // Assert
        blobStorageService.Verify(
            x => x.UploadAsync(
                "commitments-replay-input",
                "archive/input.csv",
                It.Is<BinaryData>(data => data.ToString() == "123,456")),
            Times.Once);
        blobStorageService.Verify(
            x => x.DeleteAsync("commitments-replay-input", "pending/input.csv"),
            Times.Once);
    }
}
