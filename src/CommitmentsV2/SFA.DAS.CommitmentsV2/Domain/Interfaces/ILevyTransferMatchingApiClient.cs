using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ILevyTransferMatchingApiClient
{
    Task<PledgeApplication> GetPledgeApplication(int id);
}