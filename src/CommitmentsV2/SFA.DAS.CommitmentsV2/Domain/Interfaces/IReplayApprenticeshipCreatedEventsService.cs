using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IReplayApprenticeshipCreatedEventsService
{
    Task ReplayFromFile(ReplayInputFile replayInputFile);
}
