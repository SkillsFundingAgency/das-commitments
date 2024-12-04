using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IUlnUtilisationService
{
    Task<UlnUtilisation[]> GetUlnUtilisations(string uln, CancellationToken cancellationToken);
}