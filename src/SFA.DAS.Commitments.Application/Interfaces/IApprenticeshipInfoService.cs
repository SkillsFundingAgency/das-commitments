using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IApprenticeshipInfoService
    {
        Task<StandardsView> GetStandardsAsync(bool refreshCache = false);
        Task<FrameworksView> GetFrameworksAsync(bool refreshCache = false);
        Task<ITrainingProgramme> GetTrainingProgramAsync(string id, bool refreshCache = false);
    }
}
