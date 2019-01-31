using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IApprenticeshipInfoService
    {
        Task<StandardsView> GetStandards(bool refreshCache = false);
        Task<FrameworksView> GetFrameworks(bool refreshCache = false);
        Task<ITrainingProgramme> GetTrainingProgram(string id);
    }
}
