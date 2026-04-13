using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IReplayInputFileStore
{
    Task<IReadOnlyCollection<ReplayInputFile>> GetPendingFiles();
    Task ArchiveProcessedFile(ReplayInputFile replayInputFile);
}
