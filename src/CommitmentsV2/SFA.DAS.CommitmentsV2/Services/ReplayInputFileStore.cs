using System.IO;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class ReplayInputFileStore(
    IBlobStorageService blobStorageService,
    ILogger<ReplayInputFileStore> logger)
    : IReplayInputFileStore
{
    private const string CsvExtension = ".csv";
    private const string ReplayContainer = "commitments-replay-input";
    private const string InputPrefix = "pending/";
    private const string ArchivePrefix = "archive/";

    public async Task<IReadOnlyCollection<ReplayInputFile>> GetPendingFiles()
    {
        await blobStorageService.EnsureContainerExistsAsync(ReplayContainer);
        var blobs = await blobStorageService.ListBlobsAsync(ReplayContainer, InputPrefix);

        var replayFiles = new List<ReplayInputFile>();
        foreach (var blobPath in blobs)
        {
            var extension = Path.GetExtension(blobPath);
            if (!string.Equals(extension, CsvExtension, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "Replay file {FilePath} skipped. Only CSV files are supported.",
                    blobPath);
                continue;
            }

            var fileContent = await blobStorageService.GetBlobAsync(ReplayContainer, blobPath);
            replayFiles.Add(new ReplayInputFile
            {
                Name = Path.GetFileName(blobPath),
                FullPath = blobPath,
                Content = fileContent.ToString()
            });
        }

        return replayFiles;
    }

    public async Task ArchiveProcessedFile(ReplayInputFile replayInputFile)
    {
        var archivePath = BuildArchivePath(replayInputFile.Name);

        await blobStorageService.UploadAsync(
            ReplayContainer,
            archivePath,
            BinaryData.FromString(replayInputFile.Content));

        await blobStorageService.DeleteAsync(
            ReplayContainer,
            replayInputFile.FullPath);

        logger.LogInformation(
            "Archived replay input file {SourcePath} to {ArchiveContainer}/{ArchivePath}.",
            replayInputFile.FullPath,
            ReplayContainer,
            archivePath);
    }

    private string BuildArchivePath(string fileName)
    {
        return $"{ArchivePrefix.TrimEnd('/')}/{fileName}";
    }
}
