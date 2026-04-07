using System.IO;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class ReplayInputFileStore(
    CommitmentsV2Configuration configuration,
    IBlobStorageService blobStorageService,
    ILogger<ReplayInputFileStore> logger)
    : IReplayInputFileStore
{
    private const string CsvExtension = ".csv";
    private readonly ReplayApprenticeshipCreatedEventsConfiguration _settings = configuration.ReplayApprenticeshipCreatedEvents ?? new();

    public async Task<IReadOnlyCollection<ReplayInputFile>> GetPendingFiles()
    {
        if (string.IsNullOrWhiteSpace(_settings.InputContainer))
        {
            logger.LogWarning("Replay job input container is not configured.");
            return [];
        }

        await blobStorageService.EnsureContainerExistsAsync(_settings.InputContainer);
        var blobs = await blobStorageService.ListBlobsAsync(_settings.InputContainer, _settings.InputPrefix);

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

            var fileContent = await blobStorageService.GetBlobAsync(_settings.InputContainer, blobPath);
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
        var archiveContainer = string.IsNullOrWhiteSpace(_settings.ArchiveContainer) ? _settings.InputContainer : _settings.ArchiveContainer;
        var archivePath = BuildArchivePath(replayInputFile.Name);

        await blobStorageService.UploadAsync(
            archiveContainer,
            archivePath,
            BinaryData.FromString(replayInputFile.Content));

        await blobStorageService.DeleteAsync(
            _settings.InputContainer,
            replayInputFile.FullPath);

        logger.LogInformation(
            "Archived replay input file {SourcePath} to {ArchiveContainer}/{ArchivePath}.",
            replayInputFile.FullPath,
            archiveContainer,
            archivePath);
    }

    private string BuildArchivePath(string fileName)
    {
        return string.IsNullOrWhiteSpace(_settings.ArchivePrefix) ? fileName : $"{_settings.ArchivePrefix.TrimEnd('/')}/{fileName}";
    }
}
